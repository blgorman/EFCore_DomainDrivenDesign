# Clip07 - EF Materialization
EF materializes aggregate shape without needing public mutation paths.

## Demo Class
- DemonstrateEFMaterialization.ShowMaterializationWithProof

## What To Verify
- Reloaded aggregate has lines populated without public setter.
- DomainEvents count after reload proves factory was not called by EF materialization.
- The aggregate keeps a non-public EF materialization constructor (`private Order() { Total = null!; }` after Clip 4 adds Money).

## Tests
No new domain changes are made in this clip; no new tests apply.

## Student Changes

None. There are no `//TODO: Module 2 Clip 7` markers anywhere in the solution. This is a
verification clip: run the materialization demo and read what it prints.
