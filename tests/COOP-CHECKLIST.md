# Live co-op acceptance checks

These require two players with VSS 0.2.0, plus VSS on the host/server. The scripted diagnostics do not replace this session.

- Build a pedestal for exactly 5 wood without a workbench. Rejoin and confirm it remains interactable.
- Put each supported chest type inside 20m and another outside 20m. Verify inclusion uses distance from the pedestal, including height.
- Open the grid with both players. Search, sort by quantity, and scroll with a populated network.
- Drag items in both directions, into specific player slots, and onto inventory background. Test Shift/Ctrl clicks and half-stack drags.
- Fill every chest slot with partial wood stacks. Deposit wood and verify those stacks fill without adding slots. Repeat with full stacks and incompatible item types.
- Set different chest preferences; confirm the other player sees the icon/category and new stacks route in the documented order. Rejoin to verify persistence.
- Both players withdraw the same last stack at once. Count the total across both player inventories and every chest before and after. It must remain unchanged.
- Both players deposit at once into the same nearly full chest. Confirm no duplication, lost quantity or overwritten stack.
- Keep a chest open normally on one player while the other uses the grid. That chest must reject grid transfers until the normal inventory closes.
- Open a chest normally during a grid transfer. Its normal open/stack/take-all requests must honor the temporary lease.
- Change chest contents normally while another player has the grid open. Confirm the display refreshes and a stale drag cannot create items.
- Disconnect, close the grid or die while waiting for ownership. Items must stay in the player's inventory if no commit occurred; the chest becomes available after lease expiry.
- Test ward-denied access, another player's personal chest, moving outside pedestal interaction range, destroying a pedestal/chest during a pending request, and reopening the UI after returning to the menu.
- Repeat owner handoffs with a dedicated server and both client players. Change the server radius and verify clients use the new value after reconnect/restart.
- Check BepInEx/LogOutput.log for errors, then save/reload the world and both characters to confirm persisted quantities and preferences.
