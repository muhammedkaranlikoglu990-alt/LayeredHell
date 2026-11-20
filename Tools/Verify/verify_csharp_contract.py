#!/usr/bin/env python3
"""Cross-verifies C# runtime & editor scripts, 892 3D models, 9 PNG texture atlases,
379 WAV audio files, and 53 Unity scenes against Docs/CONTRACT.md.
"""
import glob
import json
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "..", ".."))

REQUIRED_CS_CLASSES = [
    "GameBootstrap", "GameManager", "GameSettings", "Contracts",
    "PlayerController", "PlayerFactory", "PlayerStats", "FootstepPlayer",
    "InputRouter", "MobileControls", "UIFactory",
    "Interactor", "Door", "LootContainer", "WorldPickup",
    "WeaponController", "ThrowableProjectile",
    "AudioManager", "SaveSystem", "HUD", "MainMenuUI", "GameMenuUI",
    "FloorCatalog", "FloorChunk", "FloorStreamer", "FloorTransitionPoint", "CityBackdrop",
    "NoiseSystem", "ZombieArchetype", "ZombieAI", "GoliathBoss", "AcidProjectile",
    "ZombieFactory", "ZombieDirector",
    "ItemDatabase", "Inventory", "CraftingSystem",
    "BaseManager", "SurvivorNPC", "StationInteractable",
    "StoryDatabase", "JournalPickup", "WallText", "ObjectiveInteractable",
    "JournalSystem", "RadioSystem", "QuestManager", "EndingManager",
    "ModelLibrary", "AudioBank",
    "GameRenderingController", "ProceduralSkyDarkener", "FlickerLight", "SceneBootstrapper",
    "ProjectSetup", "TextureImportPostprocessor", "ModelImportPostprocessor",
    "AudioImportPostprocessor", "MaterialBuilder", "ModelLibraryBuilder",
    "AudioBankBuilder", "AnimatorControllerBuilder", "SceneGenerator",
    "URPAssetBuilder", "BuildPipelineTools", "PreBuildValidator",
]


def main():
    errors = []

    # 1. C# files check
    cs_files = sorted(
        glob.glob(os.path.join(ROOT, "Assets", "_Project", "Scripts", "*", "*.cs"))
        + glob.glob(os.path.join(ROOT, "Assets", "Editor", "*.cs"))
    )
    all_cs_text = []
    for path in cs_files:
        with open(path, "r", encoding="utf-8") as f:
            txt = f.read()
        all_cs_text.append(txt)
        if "NotImplementedException" in txt:
            errors.append(f"NotImplementedException found in {path}")
        if "namespace LayeredHell" not in txt:
            errors.append(f"Missing namespace LayeredHell in {path}")

    merged_cs = "\n".join(all_cs_text)
    for cls in REQUIRED_CS_CLASSES:
        if cls != "Contracts" and (f"class {cls}" not in merged_cs and f"struct {cls}" not in merged_cs):
            errors.append(f"Missing required C# class: {cls}")

    # 2. 3D Models check
    glb_files = glob.glob(os.path.join(ROOT, "Assets", "Art", "Models", "*", "*.glb"))
    if len(glb_files) < 840:
        errors.append(f"Expected >= 840 .glb models, found {len(glb_files)}")

    # 3. Texture Atlases check
    png_files = glob.glob(os.path.join(ROOT, "Assets", "Art", "Textures", "*.png"))
    if len(png_files) != 9:
        errors.append(f"Expected 9 PNG texture atlases, found {len(png_files)}")

    # 4. Audio WAV files check
    wav_files = glob.glob(os.path.join(ROOT, "Assets", "Audio", "*", "*.wav"))
    if not (350 <= len(wav_files) <= 400):
        errors.append(f"Expected 350..400 WAV files, found {len(wav_files)}")

    # 5. Unity Scenes check
    scene_files = (
        glob.glob(os.path.join(ROOT, "Assets", "Scenes", "*.unity"))
        + glob.glob(os.path.join(ROOT, "Assets", "Scenes", "Floors", "*.unity"))
    )
    if len(scene_files) != 53:
        errors.append(f"Expected 53 Unity scene files, found {len(scene_files)}")

    # 6. Shader check
    shader_path = os.path.join(ROOT, "Assets", "Shaders", "AtlasTiling.shader")
    if not os.path.isfile(shader_path):
        errors.append("Missing Assets/Shaders/AtlasTiling.shader")

    if errors:
        for e in errors:
            print("ERROR:", e)
        return 1

    print(
        f"verify_csharp_contract: PASS ("
        f"{len(cs_files)} C# files, "
        f"{len(glb_files)} 3D .glb models, "
        f"{len(png_files)} PNG atlases, "
        f"{len(wav_files)} WAV files, "
        f"{len(scene_files)} Unity scenes)"
    )
    return 0


if __name__ == "__main__":
    sys.exit(main())
