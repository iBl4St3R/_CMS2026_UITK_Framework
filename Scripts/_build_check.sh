#!/usr/bin/env bash
# Sprawdza, czy projekt się kompiluje względem obecnych assembly interop gry (CMS 2027), BEZ kopiowania DLL do folderu gry.
# Użycie (Git Bash, z roota repo): bash Scripts/_build_check.sh
# Bierze csproj, usuwa target CopyToMods, kompiluje *.cs w %TEMP%/<repo>_buildcheck. Paczki NuGet (np. Roslyn) przywraca dotnet.
# Szim NullableAttribute dodawany tylko, gdy repo go nie ma (obejście dla nowszego Roslyna z SDK; w VS niepotrzebny).
set -euo pipefail

REPO="${REPO_OVERRIDE:-$(cd "$(dirname "$0")/.." && pwd -W)}"   # pwd -W: ścieżka Windows (Git Bash)
PROJ="$REPO"                                              # folder z csproj
NAME="$(basename "$REPO")"
CSPROJ="$(ls "$PROJ"/*.csproj | head -1)"
DIR="${TEMP:-/tmp}/${NAME}_buildcheck"
mkdir -p "$DIR"
rm -f "$DIR"/*.csproj

sed -e '/<Target Name="CopyToMods"/,/<\/Target>/d'     -e '0,/<PropertyGroup>/s//<PropertyGroup><EnableDefaultCompileItems>false<\/EnableDefaultCompileItems>/'     -e "s#</Project>#  <ItemGroup><Compile Include=\"$PROJ/**/*.cs\" Exclude=\"$PROJ/bin/**;$PROJ/obj/**;$REPO/Scripts/**;$REPO/Samples/**\" /><Compile Include=\"Shim.cs\" Condition=\"Exists('Shim.cs')\" /></ItemGroup>\n</Project>#"     "$CSPROJ" > "$DIR/buildcheck.csproj"

rm -f "$DIR/Shim.cs"
if ! grep -rqs "class NullableAttribute" --include=*.cs "$PROJ"; then
  printf 'namespace System.Runtime.CompilerServices { internal sealed class NullableAttribute : Attribute { public NullableAttribute(byte b){} public NullableAttribute(byte[] b){} } internal sealed class NullableContextAttribute : Attribute { public NullableContextAttribute(byte b){} } }\n' > "$DIR/Shim.cs"
fi

cd "$DIR" && dotnet build -nologo -v q 2>&1 | grep -E "error|Kompil|Build succeeded|FAILED" | sed 's/ \[.*csproj\]//' | sort -u
