# ValheimCraftingSkill

A simple Valheim mod which adds a crafting skill to the game.

### Requirements:

- [BepInEx](https://github.com/BepInEx/BepInEx) ([thunderstore](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/))
- [Skill Injector](https://github.com/pipakin/PipakinsMods/tree/master/SkillInjector) ([nexus](https://www.nexusmods.com/valheim/mods/341))
- [optional] [Mod Config Enforcer](https://github.com/Pfhoenix/ModConfigEnforcer) ([nexus](https://www.nexusmods.com/valheim/mods/460))

### Configuration

The config file is created after first run. You can probably find it in `.\BepInEx\config\` directory.

`____Start` and `____Stop` config settings map skill level to item attributes bounded by the start and stop. The start value is what the attribute will be at level 0 and Stop at level 100.

`Exp____` configures the rate you gain experience at. Only the `Linear` setting is recomended to change, increase for more Exp, decrease for less.

`TierModifier` affects experience gain also but dependant on the work station. Increase the number to give a **big** boost to items from that workstation. (These settings are here mostly just to balance Troll Armour)

Default configuration:

- ArmorStart/Stop:            80% to 130%
- WeightStart/Stop:          110% to  70%
- MaxDurabilityStart/Stop:    80% to 160%
- BaseBlockPowerStart/Stop:   80% to 130%
- DeflectionForceStart/Stop:  80% to 130%
- DamageStart/Stop:           80% to 130%

- ExpScapeTier               1.00
- ExpScapePower              2.00
- ExpScapeLinear             0.16

- TierModifierInventory     0
- TierModifierWorkbench     0
- TierModifierForge         2
- TierModifierCauldron      3
- TierModifierStonecutter   1
- TierModifierArtisan       2
- TierModifierDefault       0

### Todo

#### Features:

* Variance should never decrease
* 'Reforge' an item at max quality to reroll variance and update craft level
* Armour number rounding in inventory

#### Known Bugs

* Dropped stack size gets reset to 1 when picked up
* Random game crash. Unknown cause.
* NullPointerException in log. Doesn't appear to cause issues except for error message:
```
    [Error  : Unity Log] NullReferenceException: Object reference not set to an instance of an object
    Stack trace:
    Humanoid.SetupVisEquipment (VisEquipment visEq, System.Boolean isRagdoll) (at <260a7c02345c4086b640f239744206f0>:0)
    Player.SetupVisEquipment (VisEquipment visEq, System.Boolean isRagdoll) (at <260a7c02345c4086b640f239744206f0>:0)
    ExtendedItemDataFramework.Player_Patch+WatchExtendedItemDataOnEquipment_Player_Update_Patch.Postfix (Player __instance) (at <67fcacf4676c4bb083c4a471673662ad>:0)
    (wrapper dynamic-method) Player.DMD<Player::Update>(Player)
```
* Enabling mod will attach 0 quality to items which don't require a crafting station (stone weapons, torch, hammer, etc.)


### Code inspired by:

- For general project layout and config system: [SailingSkill by gaijinx](https://github.com/gaijinx/valheim_mods/tree/main/sailing_skill)
- For examples of *using* ExtendedItemData: [EpicLoot by RandyKnapp](https://github.com/RandyKnapp/ValheimMods/tree/main/EpicLoot)
- For custom icon handling: [GatheringSkillMod by Pipakin](https://github.com/pipakin/PipakinsMods/tree/master/GatheringSkillMod)

GitHub Link: [/AnnanFay/ValheimCraftingSkill](https://github.com/AnnanFay/ValheimCraftingSkill)
