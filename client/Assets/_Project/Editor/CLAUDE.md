# Editor

Editor-only tooling for the project: import pipelines, validators, menu commands. One asmdef, `PlowParty.Editor`, compiled only in the Editor and referenced by nothing at runtime. Each tool gets a sub-folder.

## Entry points

- `ArtImport/ArtImportPostprocessor` — enforces the import rules in `Art/CLAUDE.md` for every model and texture under `Art/`. The rules live here as code, so a rebuilt or new asset needs no manual Inspector setup.

## Rules

- Runtime assemblies never reference this one; anything a build needs belongs in a runtime module.
- A tool that changes assets in bulk is a menu command or an import hook, never code that runs on Editor load.
