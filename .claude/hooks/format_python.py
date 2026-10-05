import json
import subprocess
import sys
from pathlib import Path

BACKEND = Path(__file__).resolve().parents[2] / "backend"


def main() -> int:
    payload = json.load(sys.stdin)
    file_path = Path(payload.get("tool_input", {}).get("file_path", ""))
    if file_path.suffix != ".py" or BACKEND not in file_path.resolve().parents:
        return 0
    for args in (["ruff", "check", "--fix", "--quiet"], ["ruff", "format", "--quiet"]):
        subprocess.run(["uv", "run", "--project", str(BACKEND), *args, str(file_path)], check=False)
    return 0


if __name__ == "__main__":
    sys.exit(main())
