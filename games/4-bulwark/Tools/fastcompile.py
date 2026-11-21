#!/usr/bin/env python3
"""Fast C# type check without Unity (MASTER_PROMPT 3.2 #3, "fast compile loop"). Imported from Thread's
Tools/fastcompile.py with The Ring's fastcheck.py features and the Abyss per-run idea merged in.

Generates SDK-style projects that mirror the Megastructure assemblies (Runtime incl. the Beats asmref, Editor, Tests)
against the engine assemblies Unity listed in Assembly-CSharp-Editor.csproj, then builds them with the dotnet SDK that
ships inside the Unity editor.
  * Runtime is compiled WITHOUT UNITY_EDITOR (player semantics) and without UnityEditor references.
  * Package DLLs are filtered by each asmdef's "references" (catches a missing asmdef reference, e.g. GPUDriven).
  * Separate projects with ProjectReference: Runtime calling Editor code fails like in Unity.
  * Every run writes to its own folder Tools/fastcompile/runs/<id>/ (FC_RUN env var or pid + time), so parallel runs
    (several builders) never collide; the folder is deleted afterwards unless --keep.
  * mscorlib: taken from the csproj (Unity's unity-4.8-api reference assembly); if the csproj has none, the netfx
    compat shim (NetStandard/compat/2.1.0/shims/netfx/mscorlib.dll) is added so net472 assemblies such as nunit resolve.
This is a pre-filter only: it is blind to shaders and to Unity's own asmdef rules. Always finish with a real Unity
compile (Tools/unity-queue.ps1 -Method Megastructure.EditorTools.BatchTools.CompileCheck).

Usage: python -I Tools/fastcompile.py [runtime|editor|tests|all] [--only <folders under Assets/_Game> ...]
                                      [--extra <file.cs> ...] [--keep]
  --extra: additional source files compiled into Runtime (e.g. stubs of files owned by someone else, outside the project)
Exit code 0 = no errors.
"""
import glob
import json
import os
import re
import shutil
import subprocess
import sys
import time

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
GAME = os.path.join(ROOT, "Assets", "_Game")
UNITY = r"C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor"
DOTNET = os.path.join(UNITY, "Data", "DotNetSdk", "dotnet.exe")
MSCORLIB_SHIM = os.path.join(UNITY, "Data", "NetStandard", "compat", "2.1.0", "shims", "netfx", "mscorlib.dll")

ONLY = []
EXTRA = []


def read_refs():
    src = os.path.join(ROOT, "Assembly-CSharp-Editor.csproj")
    if not os.path.exists(src):
        print("[fastcompile] Assembly-CSharp-Editor.csproj missing: open the project in Unity once (or run a batch compile)")
        sys.exit(2)
    with open(src, "r", encoding="utf-8-sig") as f:
        text = f.read()
    refs = re.findall(r'<Reference Include="([^"]+)">\s*<HintPath>([^<]+)</HintPath>', text)
    m = re.search(r"<DefineConstants>([^<]+)</DefineConstants>", text)
    defines = m.group(1) if m else ""
    refs = [(n, p if os.path.isabs(p) else os.path.join(ROOT, p)) for n, p in refs]
    return refs, defines


def asmdef_refs(path):
    try:
        with open(path, "r", encoding="utf-8-sig") as f:
            return set(json.load(f).get("references", []))
    except (OSError, ValueError):
        return set()


def is_package_dll(path):
    p = path.replace("\\", "/").lower()
    return "/library/scriptassemblies/" in p


def is_editor_ref(name, path):
    p = path.replace("\\", "/").lower()
    n = name.lower()
    if "nunit" in n or "testrunner" in n or "testframework" in n:
        return None  # tests only
    if "unityeditor" in n or n.endswith(".editor") or ".editor." in n or "editor" in os.path.basename(p).lower():
        return True
    if "/managed/unityeditor" in p:
        return True
    return False


def filter_refs(refs, allowed_packages, editor, tests):
    out = []
    for n, p in refs:
        if n.startswith("Megastructure") or n.startswith("Assembly-CSharp"):
            continue
        if is_package_dll(p) and n not in allowed_packages:
            continue
        kind = is_editor_ref(n, p)
        if kind is None and not tests:
            continue
        if kind is True and not editor:
            continue
        out.append((n, p))
    if not any(n.lower() == "mscorlib" for n, _ in out) and os.path.exists(MSCORLIB_SHIM):
        out.append(("mscorlib", MSCORLIB_SHIM))
    return out


def write_proj(out_dir, name, sources, refs, defines, project_refs=()):
    d = os.path.join(out_dir, name)
    os.makedirs(d, exist_ok=True)
    lines = [
        '<Project Sdk="Microsoft.NET.Sdk">',
        "  <PropertyGroup>",
        "    <TargetFramework>netstandard2.1</TargetFramework>",
        "    <LangVersion>9.0</LangVersion>",
        "    <EnableDefaultItems>false</EnableDefaultItems>",
        "    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>",
        "    <NoStdLib>true</NoStdLib><NoConfig>true</NoConfig>",
        "    <DisableImplicitFrameworkReferences>true</DisableImplicitFrameworkReferences>",
        "    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>",
        "    <Nullable>disable</Nullable>",
        "    <NoWarn>0169;0649;0414;CS8632;USG0001</NoWarn>",
        f"    <AssemblyName>Fast.{name}</AssemblyName>",
        f"    <DefineConstants>{defines}</DefineConstants>",
        "    <MSBuildWarningsAsMessages>MSB3277</MSBuildWarningsAsMessages>",
        "    <OutputPath>bin\\</OutputPath>",
        "  </PropertyGroup>",
        "  <ItemGroup>",
    ]
    for s in sources:
        lines.append(f'    <Compile Include="{s}" />')
    lines.append("  </ItemGroup>")
    lines.append("  <ItemGroup>")
    seen = set()
    for n, p in refs:
        if n.lower() in seen:
            continue
        seen.add(n.lower())
        lines.append(f'    <Reference Include="{n}"><HintPath>{p}</HintPath><Private>false</Private></Reference>')
    for pr in project_refs:
        lines.append(f'    <ProjectReference Include="..\\{pr}\\{pr}.csproj" />')
    lines.append("  </ItemGroup>")
    lines.append("</Project>")
    path = os.path.join(d, name + ".csproj")
    with open(path, "w", encoding="utf-8") as f:
        f.write("\n".join(lines))
    return path


def sources(*patterns):
    out = []
    for pat in patterns:
        out.extend(glob.glob(os.path.join(GAME, pat), recursive=True))
    out = sorted(set(os.path.normpath(p) for p in out))
    if ONLY:
        keep = [os.path.normpath(os.path.join(GAME, o)) for o in ONLY]
        core = os.path.normpath(os.path.join(GAME, "Scripts", "Runtime", "Core"))
        out = [p for p in out if p.startswith(core) or any(p.startswith(k) for k in keep)]
    return out


def build(path):
    cmd = [DOTNET, "build", path, "-nologo", "-v:q", "-clp:ErrorsOnly;NoSummary", "-p:WarningLevel=0", "-nodeReuse:false"]
    env = dict(os.environ, DOTNET_CLI_UI_LANGUAGE="en", VSLANG="1033", DOTNET_NOLOGO="1", DOTNET_CLI_TELEMETRY_OPTOUT="1")
    r = subprocess.run(cmd, capture_output=True, text=True, encoding="utf-8", errors="replace", env=env, cwd=os.path.dirname(path))
    out = (r.stdout or "") + (r.stderr or "")
    errs = [l for l in out.splitlines() if ": error " in l or " error " in l.lower()]
    return r.returncode, errs, out


def main():
    argv = sys.argv[1:]
    keep = "--keep" in argv
    argv = [a for a in argv if a != "--keep"]
    if "--extra" in argv:
        k = argv.index("--extra")
        j = k + 1
        while j < len(argv) and not argv[j].startswith("--"):
            EXTRA.append(os.path.abspath(argv[j]))
            j += 1
        argv = argv[:k] + argv[j:]
    if "--only" in argv:
        k = argv.index("--only")
        ONLY.extend(a.replace("\\", "/").strip("/") for a in argv[k + 1:])
        argv = argv[:k]
    which = argv[0] if argv else "all"

    run_id = os.environ.get("FC_RUN") or f"{os.getpid()}_{int(time.time() * 1000) % 100000000}"
    out_dir = os.path.join(ROOT, "Tools", "fastcompile", "runs", os.path.basename(run_id))
    os.makedirs(out_dir, exist_ok=True)

    refs, defines = read_refs()
    rt_defines = ";".join(d for d in defines.split(";") if not d.startswith("UNITY_EDITOR") and d != "UNITY_INCLUDE_TESTS")
    rt_asm = os.path.join(GAME, "Scripts", "Runtime", "Megastructure.Runtime.asmdef")
    ed_asm = os.path.join(GAME, "Scripts", "Editor", "Megastructure.Editor.asmdef")
    ts_asm = os.path.join(GAME, "Scripts", "Tests", "EditMode", "Megastructure.Tests.EditMode.asmdef")
    rt_refs = filter_refs(refs, asmdef_refs(rt_asm), editor=False, tests=False)
    ed_refs = filter_refs(refs, asmdef_refs(ed_asm), editor=True, tests=False)
    ts_refs = filter_refs(refs, asmdef_refs(ts_asm) | asmdef_refs(ed_asm), editor=True, tests=True)

    rt_src = sources("Scripts/Runtime/**/*.cs", "Beats/**/*.cs") + EXTRA
    projects = [("Runtime", write_proj(out_dir, "Runtime", rt_src, rt_refs, rt_defines))]
    if which in ("editor", "all", "tests"):
        # the Editor assembly sees Runtime through a ProjectReference compiled with player defines; Unity compiles
        # Runtime with editor defines too, so editor-only code in Runtime is type-checked by the RuntimeEd project below
        projects.append(("RuntimeEd", write_proj(out_dir, "RuntimeEd", rt_src, filter_refs(refs, asmdef_refs(rt_asm), editor=True, tests=False), defines)))
        projects.append(("Editor", write_proj(out_dir, "Editor", sources("Scripts/Editor/**/*.cs"), ed_refs, defines, ["RuntimeEd"])))
    if which in ("tests", "all"):
        projects.append(("Tests", write_proj(out_dir, "Tests", sources("Scripts/Tests/EditMode/**/*.cs"), ts_refs, defines, ["RuntimeEd", "Editor"])))
        pm_asm = os.path.join(GAME, "Scripts", "Tests", "PlayMode", "Megastructure.Tests.PlayMode.asmdef")
        projects.append(("PlayTests", write_proj(out_dir, "PlayTests", sources("Scripts/Tests/PlayMode/**/*.cs"),
                                                 filter_refs(refs, asmdef_refs(pm_asm), editor=True, tests=True), defines, ["RuntimeEd"])))
    if which == "runtime":
        projects = projects[:1]

    rc = 0
    for name, path in projects:
        code, errs, raw = build(path)
        seen, uniq = set(), []
        for e in errs:
            k = re.sub(r"\s*\[[^\]]*\.csproj\]$", "", e).strip()
            if k not in seen:
                seen.add(k)
                uniq.append(k.replace(ROOT + os.sep, ""))
        print(f"[fastcompile] {name}: {'OK' if code == 0 else 'FAILED'} ({len(uniq)} error lines)")
        for e in uniq[:150]:
            print("  " + e)
        if code != 0 and not uniq:
            print(raw[-3000:])
        if code != 0:
            rc = 1
            if name in ("Runtime", "RuntimeEd"):
                break   # dependants would only repeat the failure
    if not keep:
        shutil.rmtree(out_dir, ignore_errors=True)
    return rc


if __name__ == "__main__":
    sys.exit(main())
