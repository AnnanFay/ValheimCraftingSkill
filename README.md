# ValheimCraftingSkill

A simple Valheim mod which adds a crafting skill to the game.

As you level up the crafting skill items you create will become better quality (increased durability, damage, etc). Craft quality is attached to item data so other players with the mod installed will gain the affects. By default your skill level is the only element which affects item stats with most increases going from -20% at level 1 to +30% at level 100.

"StochasticVariance" can be enabled in the settings to add a random element to every crafted item. "QuantisedQuality" lets you replace gradual increases (levels 1 to 100) with fixed thresholds (Normal, Fine, Superior, etc.)


### Requirements

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

#### Features to add

* 'Reforge' an item at max quality to reroll variance and update craft level

#### Known Bugs

* [MAJOR ISSUE] Dropped stack size gets set to 1 when picked up in multiplayer games (looking into fixing asap)
* Nails get attached quality (doesn't do anything), and items genereted by deconstructing a building have no quality (nails)
* Enabling mod will attach 0 quality to items which don't require a crafting station (stone weapons, torch, hammer, etc.)
* Random game crash. Unknown cause. See #1 (please report if you get this!)
* NullPointerException in log whenever player equips or unequips items. Doesn't appear to cause issues except for error message:
```
    [Error  : Unity Log] NullReferenceException: Object reference not set to an instance of an object
    Stack trace:
    Humanoid.SetupVisEquipment (VisEquipment visEq, System.Boolean isRagdoll) (at <260a7c02345c4086b640f239744206f0>:0)
    Player.SetupVisEquipment (VisEquipment visEq, System.Boolean isRagdoll) (at <260a7c02345c4086b640f239744206f0>:0)
    ExtendedItemDataFramework.Player_Patch+WatchExtendedItemDataOnEquipment_Player_Update_Patch.Postfix (Player __instance) (at <67fcacf4676c4bb083c4a471673662ad>:0)
    (wrapper dynamic-method) Player.DMD<Player::Update>(Player)
```
* Fire/Frost arrows may give too much Experience

### Code inspired by

- For general project layout and config system: [SailingSkill by gaijinx](https://github.com/gaijinx/valheim_mods/tree/main/sailing_skill)
- For examples of *using* ExtendedItemData: [EpicLoot by RandyKnapp](https://github.com/RandyKnapp/ValheimMods/tree/main/EpicLoot)
- For custom icon handling: [GatheringSkillMod by Pipakin](https://github.com/pipakin/PipakinsMods/tree/master/GatheringSkillMod)

GitHub Link: [/AnnanFay/ValheimCraftingSkill](https://github.com/AnnanFay/ValheimCraftingSkill)
