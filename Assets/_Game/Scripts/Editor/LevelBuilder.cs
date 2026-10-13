using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

namespace DontTouchTheFloor.Editor
{
    public static class LevelBuilder
    {
        [MenuItem("Don't Touch The Floor/Build Level 01")]
        public static void BuildLevel01()
        {
            Debug.Log("[LevelBuilder] Starting Level 01 generation...");

            // 1. Ensure directories exist
            EnsureDirectories();

            // 2. Create Materials
            var materials = CreateMaterials();

            // 3. Create a new scene
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 4. Build Environment Root
            GameObject envRoot = new GameObject("Environment");

            // 4a. Room Structure (Walls & Ceiling)
            BuildRoomShell(envRoot.transform, materials);

            // 4b. Lava Floor
            BuildLavaFloor(envRoot.transform, materials);

            // 4c. Start Safe Platform
            GameObject startPlatform = BuildStartPlatform(envRoot.transform, materials);

            // 4d. Bedroom Furniture Platforms
            BuildFurniturePlatforms(envRoot.transform, materials);

            // 4e. Exit Door & Trigger
            GameObject exitDoorObj = BuildExitDoor(envRoot.transform, materials);

            // 5. Lighting
            BuildLighting(envRoot.transform);

            // 6. Spawn Point
            GameObject spawnPoint = new GameObject("SpawnPoint");
            spawnPoint.transform.position = new Vector3(0.0f, 1.2f, -8.5f);

            // 7. Player Setup
            GameObject playerObj = BuildPlayer(spawnPoint.transform.position);

            // 8. Game Manager & UI
            GameObject gameManagerObj = new GameObject("GameManager");
            var gm = gameManagerObj.AddComponent<GameManager>();
            var ui = gameManagerObj.AddComponent<GameUI>();

            gm.player = playerObj.GetComponent<PlayerController>();
            gm.spawnPoint = spawnPoint.transform;
            gm.exitDoor = exitDoorObj.GetComponentInChildren<ExitDoor>();

            // 9. Save Scene
            string scenePath = "Assets/_Game/Scenes/Level_01.unity";
            bool saveSuccess = EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[LevelBuilder] Level 01 generated and saved to {scenePath}. Success: {saveSuccess}");
        }

        private static void EnsureDirectories()
        {
            string[] dirs = {
                "Assets/_Game/Art",
                "Assets/_Game/Audio",
                "Assets/_Game/Materials",
                "Assets/_Game/Prefabs",
                "Assets/_Game/Scenes",
                "Assets/_Game/Scripts",
                "Assets/_Game/UI"
            };
            foreach (var dir in dirs)
            {
                if (!AssetDatabase.IsValidFolder(dir))
                {
                    string parent = Path.GetDirectoryName(dir).Replace("\\", "/");
                    string folder = Path.GetFileName(dir);
                    AssetDatabase.CreateFolder(parent, folder);
                }
            }
        }

        private class MaterialSet
        {
            public Material Lava;
            public Material Wood;
            public Material Mattress;
            public Material Sofa;
            public Material Cushion;
            public Material Walls;
            public Material Rug;
            public Material Exit;
            public Material Metal;
        }

        private static MaterialSet CreateMaterials()
        {
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");

            Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
            if (unlitShader == null) unlitShader = Shader.Find("Unlit/Color");

            MaterialSet set = new MaterialSet();

            // Lava Material (Emissive bright red-orange)
            set.Lava = GetOrCreateMaterial("Mat_Lava", litShader, new Color(1.0f, 0.22f, 0.05f));
            set.Lava.EnableKeyword("_EMISSION");
            set.Lava.SetColor("_EmissionColor", new Color(1.5f, 0.35f, 0.05f));

            // Wood Material
            set.Wood = GetOrCreateMaterial("Mat_Wood", litShader, new Color(0.42f, 0.26f, 0.15f));

            // Mattress Material
            set.Mattress = GetOrCreateMaterial("Mat_Mattress", litShader, new Color(0.88f, 0.90f, 0.95f));

            // Sofa Material (Navy/Indigo)
            set.Sofa = GetOrCreateMaterial("Mat_Sofa", litShader, new Color(0.20f, 0.42f, 0.65f));

            // Cushion Material (Warm Yellow)
            set.Cushion = GetOrCreateMaterial("Mat_Cushion", litShader, new Color(0.95f, 0.68f, 0.20f));

            // Wall Material (Warm cream beige)
            set.Walls = GetOrCreateMaterial("Mat_Walls", litShader, new Color(0.83f, 0.81f, 0.77f));

            // Rug Material (Safe green tint)
            set.Rug = GetOrCreateMaterial("Mat_Rug", litShader, new Color(0.28f, 0.58f, 0.36f));

            // Exit Glow Material (Vibrant neon green)
            set.Exit = GetOrCreateMaterial("Mat_Exit", litShader, new Color(0.15f, 0.95f, 0.35f));
            set.Exit.EnableKeyword("_EMISSION");
            set.Exit.SetColor("_EmissionColor", new Color(0.3f, 1.8f, 0.7f));

            // Metal / White Trim
            set.Metal = GetOrCreateMaterial("Mat_Metal", litShader, new Color(0.92f, 0.92f, 0.94f));

            AssetDatabase.SaveAssets();
            return set;
        }

        private static Material GetOrCreateMaterial(string name, Shader shader, Color color)
        {
            string path = $"Assets/_Game/Materials/{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                mat.color = color;
                if (mat.HasProperty("_BaseColor"))
                {
                    mat.SetColor("_BaseColor", color);
                }
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                mat.shader = shader;
                mat.color = color;
                if (mat.HasProperty("_BaseColor"))
                {
                    mat.SetColor("_BaseColor", color);
                }
                EditorUtility.SetDirty(mat);
            }
            return mat;
        }

        private static void BuildRoomShell(Transform parent, MaterialSet mats)
        {
            GameObject room = new GameObject("RoomShell");
            room.transform.SetParent(parent);

            // Ceiling (Y = 5.0)
            CreateBox("Ceiling", room.transform, new Vector3(0f, 5.0f, 0f), new Vector3(14f, 0.2f, 21f), mats.Walls);

            // Back Wall (Z = -10.5)
            CreateBox("Wall_Back", room.transform, new Vector3(0f, 2.5f, -10.5f), new Vector3(14f, 5.2f, 0.4f), mats.Walls);

            // Front Wall with Doorway cutout (Z = 10.5)
            CreateBox("Wall_Front_Left", room.transform, new Vector3(-4.0f, 2.5f, 10.5f), new Vector3(6.0f, 5.2f, 0.4f), mats.Walls);
            CreateBox("Wall_Front_Right", room.transform, new Vector3(4.0f, 2.5f, 10.5f), new Vector3(6.0f, 5.2f, 0.4f), mats.Walls);
            CreateBox("Wall_Front_Top", room.transform, new Vector3(0.0f, 4.7f, 10.5f), new Vector3(2.5f, 0.8f, 0.4f), mats.Walls);

            // Left Wall (X = -7.0)
            CreateBox("Wall_Left", room.transform, new Vector3(-7.0f, 2.5f, 0f), new Vector3(0.4f, 5.2f, 21f), mats.Walls);

            // Right Wall (X = 7.0)
            CreateBox("Wall_Right", room.transform, new Vector3(7.0f, 2.5f, 0f), new Vector3(0.4f, 5.2f, 21f), mats.Walls);
        }

        private static void BuildLavaFloor(Transform parent, MaterialSet mats)
        {
            GameObject lavaObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lavaObj.name = "LavaFloor";
            lavaObj.transform.SetParent(parent);
            lavaObj.transform.position = new Vector3(0f, 0.0f, 0f);
            lavaObj.transform.localScale = new Vector3(14.0f, 0.3f, 21.0f);

            var renderer = lavaObj.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = mats.Lava;

            // Make the collider a trigger for instant detection
            var col = lavaObj.GetComponent<BoxCollider>();
            col.isTrigger = true;

            lavaObj.AddComponent<LavaFloor>();
        }

        private static GameObject BuildStartPlatform(Transform parent, MaterialSet mats)
        {
            GameObject startRoot = new GameObject("StartPlatform_Rug");
            startRoot.transform.SetParent(parent);

            // Wood border/base
            CreateBox("WoodLedge", startRoot.transform, new Vector3(0f, 0.25f, -8.5f), new Vector3(3.2f, 0.3f, 2.6f), mats.Wood);
            // Soft carpet rug top
            CreateBox("Carpet", startRoot.transform, new Vector3(0f, 0.42f, -8.5f), new Vector3(2.8f, 0.1f, 2.2f), mats.Rug);

            return startRoot;
        }

        private static void BuildFurniturePlatforms(Transform parent, MaterialSet mats)
        {
            GameObject furnitureRoot = new GameObject("Furniture");
            furnitureRoot.transform.SetParent(parent);

            // 1. Bed (Left Side, Z: -5.0)
            GameObject bed = new GameObject("Furniture_Bed");
            bed.transform.SetParent(furnitureRoot.transform);
            CreateBox("Frame", bed.transform, new Vector3(-2.8f, 0.35f, -5.0f), new Vector3(2.6f, 0.5f, 3.8f), mats.Wood);
            CreateBox("Mattress", bed.transform, new Vector3(-2.8f, 0.70f, -5.0f), new Vector3(2.4f, 0.3f, 3.6f), mats.Mattress);
            CreateBox("Pillow", bed.transform, new Vector3(-2.8f, 0.90f, -6.3f), new Vector3(1.6f, 0.15f, 0.7f), mats.Cushion);
            CreateBox("Headboard", bed.transform, new Vector3(-2.8f, 1.15f, -6.85f), new Vector3(2.6f, 1.3f, 0.2f), mats.Wood);

            // 2. Nightstand / Bedside Table (X: -4.8, Z: -2.4)
            GameObject nightstand = new GameObject("Furniture_Nightstand");
            nightstand.transform.SetParent(furnitureRoot.transform);
            CreateBox("NightstandBase", nightstand.transform, new Vector3(-4.8f, 0.5f, -2.4f), new Vector3(1.3f, 0.9f, 1.3f), mats.Wood);
            CreateBox("NightstandTop", nightstand.transform, new Vector3(-4.8f, 0.98f, -2.4f), new Vector3(1.4f, 0.08f, 1.4f), mats.Metal);

            // 3. Bedroom Chair (Intermediate hop near center: X: -1.8, Z: -2.0)
            GameObject chair = new GameObject("Furniture_Chair");
            chair.transform.SetParent(furnitureRoot.transform);
            CreateBox("Seat", chair.transform, new Vector3(-1.8f, 0.55f, -2.0f), new Vector3(1.2f, 0.12f, 1.2f), mats.Wood);
            CreateBox("Cushion", chair.transform, new Vector3(-1.8f, 0.65f, -2.0f), new Vector3(1.0f, 0.10f, 1.0f), mats.Cushion);
            CreateBox("LegFL", chair.transform, new Vector3(-2.3f, 0.25f, -1.5f), new Vector3(0.12f, 0.5f, 0.12f), mats.Wood);
            CreateBox("LegFR", chair.transform, new Vector3(-1.3f, 0.25f, -1.5f), new Vector3(0.12f, 0.5f, 0.12f), mats.Wood);
            CreateBox("LegBL", chair.transform, new Vector3(-2.3f, 0.25f, -2.5f), new Vector3(0.12f, 0.5f, 0.12f), mats.Wood);
            CreateBox("LegBR", chair.transform, new Vector3(-1.3f, 0.25f, -2.5f), new Vector3(0.12f, 0.5f, 0.12f), mats.Wood);
            CreateBox("Backrest", chair.transform, new Vector3(-1.8f, 1.05f, -2.55f), new Vector3(1.2f, 0.9f, 0.12f), mats.Wood);

            // 4. Large Study Table / Desk (Center: X: 0.5, Z: 0.8)
            GameObject table = new GameObject("Furniture_Table");
            table.transform.SetParent(furnitureRoot.transform);
            CreateBox("TableTop", table.transform, new Vector3(0.5f, 1.15f, 0.8f), new Vector3(2.8f, 0.18f, 2.4f), mats.Wood);
            CreateBox("Leg1", table.transform, new Vector3(-0.7f, 0.55f, -0.2f), new Vector3(0.18f, 1.1f, 0.18f), mats.Wood);
            CreateBox("Leg2", table.transform, new Vector3(1.7f, 0.55f, -0.2f), new Vector3(0.18f, 1.1f, 0.18f), mats.Wood);
            CreateBox("Leg3", table.transform, new Vector3(-0.7f, 0.55f, 1.8f), new Vector3(0.18f, 1.1f, 0.18f), mats.Wood);
            CreateBox("Leg4", table.transform, new Vector3(1.7f, 0.55f, 1.8f), new Vector3(0.18f, 1.1f, 0.18f), mats.Wood);

            // 5. Cozy Sofa / Couch (Right side: X: 3.5, Z: 3.8)
            GameObject sofa = new GameObject("Furniture_Sofa");
            sofa.transform.SetParent(furnitureRoot.transform);
            CreateBox("SofaBase", sofa.transform, new Vector3(3.5f, 0.55f, 3.8f), new Vector3(2.0f, 0.7f, 3.8f), mats.Sofa);
            CreateBox("SofaSeatCushion", sofa.transform, new Vector3(3.3f, 0.98f, 3.8f), new Vector3(1.6f, 0.2f, 3.6f), mats.Sofa);
            CreateBox("SofaBackrest", sofa.transform, new Vector3(4.35f, 1.4f, 3.8f), new Vector3(0.4f, 1.2f, 3.8f), mats.Sofa);
            CreateBox("ArmrestFront", sofa.transform, new Vector3(3.5f, 1.15f, 1.85f), new Vector3(2.0f, 0.5f, 0.35f), mats.Sofa);
            CreateBox("ArmrestBack", sofa.transform, new Vector3(3.5f, 1.15f, 5.75f), new Vector3(2.0f, 0.5f, 0.35f), mats.Sofa);

            // 6. Wall Shelves / Bookshelf (Stepping toward exit: X: 0.8, Z: 6.2 and X: -1.6, Z: 7.8)
            GameObject shelf1 = new GameObject("Furniture_Shelf_Low");
            shelf1.transform.SetParent(furnitureRoot.transform);
            CreateBox("Plank1", shelf1.transform, new Vector3(0.8f, 1.35f, 6.2f), new Vector3(2.2f, 0.2f, 1.4f), mats.Wood);
            CreateBox("SupportL1", shelf1.transform, new Vector3(0.0f, 0.65f, 6.2f), new Vector3(0.15f, 1.3f, 1.2f), mats.Wood);
            CreateBox("SupportR1", shelf1.transform, new Vector3(1.6f, 0.65f, 6.2f), new Vector3(0.15f, 1.3f, 1.2f), mats.Wood);

            GameObject shelf2 = new GameObject("Furniture_Shelf_High");
            shelf2.transform.SetParent(furnitureRoot.transform);
            CreateBox("Plank2", shelf2.transform, new Vector3(-1.6f, 1.62f, 7.8f), new Vector3(2.0f, 0.2f, 1.4f), mats.Wood);
            CreateBox("SupportL2", shelf2.transform, new Vector3(-2.4f, 0.8f, 7.8f), new Vector3(0.15f, 1.55f, 1.2f), mats.Wood);
            CreateBox("SupportR2", shelf2.transform, new Vector3(-0.8f, 0.8f, 7.8f), new Vector3(0.15f, 1.55f, 1.2f), mats.Wood);
        }

        private static GameObject BuildExitDoor(Transform parent, MaterialSet mats)
        {
            GameObject doorRoot = new GameObject("ExitDoor");
            doorRoot.transform.SetParent(parent);

            // Exit Landing Platform (Z = 9.5)
            CreateBox("LandingPlatform", doorRoot.transform, new Vector3(0f, 1.70f, 9.4f), new Vector3(2.8f, 0.35f, 1.8f), mats.Wood);
            CreateBox("LandingRug", doorRoot.transform, new Vector3(0f, 1.90f, 9.4f), new Vector3(2.4f, 0.08f, 1.4f), mats.Rug);

            // Door Frame (Z = 10.3)
            CreateBox("FrameLeft", doorRoot.transform, new Vector3(-1.1f, 3.2f, 10.3f), new Vector3(0.25f, 2.6f, 0.3f), mats.Wood);
            CreateBox("FrameRight", doorRoot.transform, new Vector3(1.1f, 3.2f, 10.3f), new Vector3(0.25f, 2.6f, 0.3f), mats.Wood);
            CreateBox("FrameTop", doorRoot.transform, new Vector3(0.0f, 4.45f, 10.3f), new Vector3(2.45f, 0.25f, 0.3f), mats.Wood);

            // Glowing Exit Portal / Finish trigger
            GameObject portal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            portal.name = "ExitTriggerZone";
            portal.transform.SetParent(doorRoot.transform);
            portal.transform.position = new Vector3(0f, 3.1f, 10.1f);
            portal.transform.localScale = new Vector3(1.9f, 2.4f, 0.4f);

            var portalRenderer = portal.GetComponent<MeshRenderer>();
            portalRenderer.sharedMaterial = mats.Exit;

            var portalCollider = portal.GetComponent<BoxCollider>();
            portalCollider.isTrigger = true;
            portalCollider.center = new Vector3(0f, 0f, -0.5f);
            portalCollider.size = new Vector3(1.0f, 1.0f, 2.0f);

            var exitDoorScript = portal.AddComponent<ExitDoor>();

            // Floating Light Beacon
            GameObject lightBeacon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lightBeacon.name = "ExitBeacon";
            lightBeacon.transform.SetParent(doorRoot.transform);
            lightBeacon.transform.position = new Vector3(0f, 3.4f, 9.8f);
            lightBeacon.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
            lightBeacon.GetComponent<MeshRenderer>().sharedMaterial = mats.Exit;
            Object.DestroyImmediate(lightBeacon.GetComponent<Collider>());

            exitDoorScript.doorLightTransform = lightBeacon.transform;

            // Point light at the door
            GameObject doorLightObj = new GameObject("DoorGreenLight");
            doorLightObj.transform.SetParent(doorRoot.transform);
            doorLightObj.transform.position = new Vector3(0f, 3.5f, 9.8f);
            Light doorLight = doorLightObj.AddComponent<Light>();
            doorLight.type = LightType.Point;
            doorLight.color = new Color(0.2f, 1.0f, 0.4f);
            doorLight.range = 6.0f;
            doorLight.intensity = 1.8f;

            return doorRoot;
        }

        private static void BuildLighting(Transform parent)
        {
            GameObject lightsRoot = new GameObject("Lighting");
            lightsRoot.transform.SetParent(parent);

            // Sun / Main Directional Light
            GameObject dirObj = new GameObject("DirectionalLight");
            dirObj.transform.SetParent(lightsRoot.transform);
            dirObj.transform.rotation = Quaternion.Euler(48.0f, -32.0f, 0f);
            Light dirLight = dirObj.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.color = new Color(1.0f, 0.94f, 0.88f);
            dirLight.intensity = 1.1f;
            dirLight.shadows = LightShadows.Soft;

            // Lava Glow Point Light (Warm red/orange bottom bounce)
            GameObject lavaLightObj = new GameObject("LavaAmbientLight");
            lavaLightObj.transform.SetParent(lightsRoot.transform);
            lavaLightObj.transform.position = new Vector3(0f, 0.8f, 0f);
            Light lavaLight = lavaLightObj.AddComponent<Light>();
            lavaLight.type = LightType.Point;
            lavaLight.color = new Color(1.0f, 0.3f, 0.05f);
            lavaLight.range = 24.0f;
            lavaLight.intensity = 1.5f;

            // Room Ceiling Ambient Light
            GameObject ceilingLightObj = new GameObject("CeilingWarmLight");
            ceilingLightObj.transform.SetParent(lightsRoot.transform);
            ceilingLightObj.transform.position = new Vector3(0f, 4.4f, 0f);
            Light ceilingLight = ceilingLightObj.AddComponent<Light>();
            ceilingLight.type = LightType.Point;
            ceilingLight.color = new Color(1.0f, 0.92f, 0.8f);
            ceilingLight.range = 16.0f;
            ceilingLight.intensity = 0.9f;
        }

        private static GameObject BuildPlayer(Vector3 spawnPosition)
        {
            GameObject player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = spawnPosition;

            var cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.4f;
            cc.center = new Vector3(0f, 0.9f, 0f);
            cc.stepOffset = 0.35f;

            var pc = player.AddComponent<PlayerController>();
            pc.moveSpeed = 6.0f;
            pc.jumpHeight = 1.45f;
            pc.gravity = -22.0f;
            pc.mouseSensitivity = 0.15f;

            // Main Camera child
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            camObj.transform.SetParent(player.transform);
            camObj.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            camObj.transform.localRotation = Quaternion.identity;

            var cam = camObj.AddComponent<Camera>();
            cam.fieldOfView = 75f;
            cam.nearClipPlane = 0.1f;
            camObj.AddComponent<AudioListener>();

            pc.cameraTransform = camObj.transform;

            return player;
        }

        private static GameObject CreateBox(string name, Transform parent, Vector3 localPos, Vector3 localScale, Material mat)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent);
            box.transform.localPosition = localPos;
            box.transform.localScale = localScale;
            if (mat != null)
            {
                box.GetComponent<MeshRenderer>().sharedMaterial = mat;
            }
            return box;
        }
    }
}
