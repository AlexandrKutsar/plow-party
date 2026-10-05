import ast
import io
import sys
import tokenize
from pathlib import Path

ROOTS = ("src", "tests", "scripts", "migrations")
DIRECTIVES = ("# noqa", "# type:", "# pyright:")
DOCSTRING_OWNERS = (ast.Module, ast.ClassDef, ast.FunctionDef, ast.AsyncFunctionDef)


def comment_lines(source: str) -> list[int]:
    tokens = tokenize.generate_tokens(io.StringIO(source).readline)
    return [
        token.start[0]
        for token in tokens
        if token.type == tokenize.COMMENT and not token.string.startswith(DIRECTIVES)
    ]


def docstring_lines(source: str) -> list[int]:
    lines: list[int] = []
    for node in ast.walk(ast.parse(source)):
        if not isinstance(node, DOCSTRING_OWNERS) or not node.body:
            continue
        first = node.body[0]
        match first:
            case ast.Expr(value=ast.Constant(value=str())):
                lines.append(first.lineno)
            case _:
                pass
    return lines


def main() -> int:
    backend = Path(__file__).resolve().parents[1]
    violations: list[str] = []
    for root in ROOTS:
        for path in sorted((backend / root).rglob("*.py")):
            source = path.read_text(encoding="utf-8")
            relative = path.relative_to(backend)
            violations += [f"{relative}:{line}: comment" for line in comment_lines(source)]
            violations += [f"{relative}:{line}: docstring" for line in docstring_lines(source)]
    for violation in violations:
        print(violation)
    return 1 if violations else 0


if __name__ == "__main__":
    sys.exit(main())
