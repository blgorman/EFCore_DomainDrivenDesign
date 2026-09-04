# Clip06 - Cross-Context Reads
Live demo, but no code to write — this demo only uses OrderingContext and never calls into ShippingContext.

## Demo Class
- DemonstrateCrossContextReads.ShowCrossContextProjectionAsync

## Tests
No unit tests are introduced in this clip.

## What To Verify
- The three options (thin query service, denormalization, cross-schema join) each show a trade-offs box.
- The live projection screen runs a real `orderingCtx.Orders.Join(orderingCtx.Customers, ...)` query and returns real seeded rows.

## Student Changes
None. No code changes for this clip.
