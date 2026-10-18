from pathlib import Path
import shutil
import zipfile
import xml.etree.ElementTree as ET

root = Path(__file__).resolve().parents[1]
stage = root / "artifacts" / "Imperium-Living-Worlds"
if stage.exists():
    shutil.rmtree(stage)
shutil.copytree(root / "Mod", stage)
ET.parse(stage / "About" / "About.xml")
assembly_dir = stage / "Assemblies"
assembly_dir.mkdir(exist_ok=True)
for project, filename in [("", "Imperium.LivingWorlds.dll"), ("Core", "Imperium.Core.dll")]:
    source = root / "Source" / project / "bin" / "Release" / "net10.0" / filename
    if not source.is_file():
        raise FileNotFoundError(f"Build Release configuration first: {source}")
    shutil.copy2(source, assembly_dir / filename)
shutil.copy2(root / "README.md", stage / "README.md")
archive = root / "artifacts" / "Imperium-Living-Worlds-dev.zip"
with zipfile.ZipFile(archive, "w", zipfile.ZIP_DEFLATED) as z:
    for path in sorted(stage.rglob("*")):
        if path.is_file() and path.name != ".gitkeep":
            z.write(path, path.relative_to(stage.parent))
print(archive)
