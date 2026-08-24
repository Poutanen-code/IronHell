# Device Semantics Analysis

Date: 2026-08-03  
Scope: G-05 and G-06 device semantics for rods, wands, and staves

## Objective

Separate device Policy from device Actions.

## Evidence Basis

- Device command flows and unstacking behavior: [reference-mangband-1_5_3/src/server/cmd6.c](reference-mangband-1_5_3/src/server/cmd6.c#L21)
- Device payload dispatch and charge or timeout logic: [reference-mangband-1_5_3/src/server/use-obj.c](reference-mangband-1_5_3/src/server/use-obj.c#L1340)
- Stacking and absorb rules: [reference-mangband-1_5_3/src/server/object2.c](reference-mangband-1_5_3/src/server/object2.c#L1590)
- Charge distribution and reduction rules: [reference-mangband-1_5_3/src/server/object2.c](reference-mangband-1_5_3/src/server/object2.c#L5124)
- Recharge behavior for wands and staves: [reference-mangband-1_5_3/src/server/spells2.c](reference-mangband-1_5_3/src/server/spells2.c#L3065)
- Destroy or split interactions: [reference-mangband-1_5_3/src/server/cmd3.c](reference-mangband-1_5_3/src/server/cmd3.c#L799)

## 1. Rods

## Timeout model

- Rods use timeout rather than consumable charges for per-use resource control.
- Availability is evaluated per stack by converting current timeout to number of rods currently charging.

Evidence:
- [reference-mangband-1_5_3/src/server/use-obj.c](reference-mangband-1_5_3/src/server/use-obj.c#L1704)

## Recharge model

- Successful rod use adds k_ptr->pval to timeout when charge is consumed.
- Full stack unavailable when charging count reaches stack size.

Evidence:
- [reference-mangband-1_5_3/src/server/use-obj.c](reference-mangband-1_5_3/src/server/use-obj.c#L1933)

## Stack behavior

- Rod stacks combine pval and timeout in absorb operations.
- Unstacking distributes pval and timeout proportionally.

Evidence:
- [reference-mangband-1_5_3/src/server/object2.c](reference-mangband-1_5_3/src/server/object2.c#L1822)
- [reference-mangband-1_5_3/src/server/object2.c](reference-mangband-1_5_3/src/server/object2.c#L5124)

## Availability rules

- If all rods are charging, action is blocked and no payload executes.

Evidence:
- [reference-mangband-1_5_3/src/server/use-obj.c](reference-mangband-1_5_3/src/server/use-obj.c#L1710)

## 2. Wands

## Charge model

- Wands use pval as charge count.
- Use consumes one charge on successful command completion.

Evidence:
- [reference-mangband-1_5_3/src/server/cmd6.c](reference-mangband-1_5_3/src/server/cmd6.c#L714)
- [reference-mangband-1_5_3/src/server/use-obj.c](reference-mangband-1_5_3/src/server/use-obj.c#L1379)

## Stack behavior

- Stack totals combine charges.
- Use may trigger unstacking, with charge distribution preserving totals.

Evidence:
- [reference-mangband-1_5_3/src/server/object2.c](reference-mangband-1_5_3/src/server/object2.c#L1831)
- [reference-mangband-1_5_3/src/server/cmd6.c](reference-mangband-1_5_3/src/server/cmd6.c#L717)

## Recharge interactions

- Recharge targets wand or staff only.
- Recharge can backfire and reduce charges or destroy item.

Evidence:
- [reference-mangband-1_5_3/src/server/spells2.c](reference-mangband-1_5_3/src/server/spells2.c#L3052)
- [reference-mangband-1_5_3/src/server/spells2.c](reference-mangband-1_5_3/src/server/spells2.c#L3130)

## 3. Staves

## Charge pool model

- Staves also use pval as pooled charges.
- One charge consumed per use.

Evidence:
- [reference-mangband-1_5_3/src/server/cmd6.c](reference-mangband-1_5_3/src/server/cmd6.c#L558)

## Group usage behavior

- Stack and unstack operations distribute charges across split stacks.
- Empty status and awareness are part of policy feedback.

Evidence:
- [reference-mangband-1_5_3/src/server/cmd6.c](reference-mangband-1_5_3/src/server/cmd6.c#L500)
- [reference-mangband-1_5_3/src/server/object2.c](reference-mangband-1_5_3/src/server/object2.c#L5124)

## 4. Policy vs Action Separation

## Device Policy Layer

1. Usability gates by skill, confusion penalty, and level difficulty.
2. Resource availability checks by charge or timeout model.
3. Stack splitting and charge or timeout redistribution.
4. Recharge risk and backfire behavior.

## Device Action Layer

- Effect payload execution after policy gates pass:
  - bolts, balls, healing, detect, teleport, status cures, terrain interactions.

Evidence:
- [reference-mangband-1_5_3/src/server/use-obj.c](reference-mangband-1_5_3/src/server/use-obj.c#L1440)

## 5. Research Conclusion

- Device payloads are broadly reusable with spell and activation payload families.
- Device economies are source-specific policies and cannot be represented as generic action parameters without parity loss.

Safe architectural boundary:
- Canonical action payload: yes.
- Canonical device policy: no, separate rod and wand and staff policy adapters are required.
