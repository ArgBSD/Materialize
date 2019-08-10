﻿#region

#region

using Materialize.General;
using Materialize.Settings;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using Utility;
using Logger = Utility.Logger;

#endregion

namespace Materialize.Gui
{

    #endregion

    public class MaterialGui : MonoBehaviour, IHideable
    {
        private const int UpdateDivisor = 4;
        private static readonly int NormalScaleId = Shader.PropertyToID("_NormalScale");
        private static readonly int MetallicMultiplierId = Shader.PropertyToID("_MetallicMultiplier");
        private static readonly int SmoothnessMultiplierId = Shader.PropertyToID("_SmoothnessMultiplier");
        private static readonly int AoMultiplierId = Shader.PropertyToID("_AoMultiplier");
        private static readonly int DisplacementStrength = Shader.PropertyToID("_DisplacementStrength");
        private static readonly int DiffuseMap = Shader.PropertyToID("_BaseColorMap");
        private ColorAdjustments _colorAdjustments;

        private bool _cubeShown;
        private bool _cylinderShown;
        private Texture2D _diffuseMap;
        private int _divisorCount = UpdateDivisor;
        private HDRISky _hdriSky;

        private Texture2D _heightMap;

        private MaterialSettings _materialSettings;

        private Texture2D _myColorTexture;

        private bool _planeShown = true;

        private bool _settingsInitialized;
        private bool _sphereShown;

        private Material _thisMaterial;
        private int _windowId;

        private Rect _windowRect;

        public GameObject TestObject;
        public GameObject TestObjectCube;
        public GameObject TestObjectCylinder;

        public GameObject TestObjectParent;
        public GameObject TestObjectSphere;
        public ObjectZoomPanRotate TestRotator;

        public bool Hide { get; set; }

        private void OnDisable()
        {
            if (!MainGui.Instance.IsGuiHidden || TestObjectParent == null) return;
            if (!TestObjectParent.activeSelf) TestRotator.Reset();

            TestObjectParent.SetActive(true);
            TestObjectCube.SetActive(false);
            TestObjectCylinder.SetActive(false);
            TestObjectSphere.SetActive(false);
        }

        private void Awake()
        {
            ProgramManager.Instance.SceneObjects.Add(gameObject);
            _windowRect = new Rect(10.0f, 265.0f, 300f, 575f);
        }

        private void Start()
        {
            InitializeSettings();
            _windowId = ProgramManager.Instance.GetWindowId;
            ProgramManager.Instance.SceneVolume.profile.TryGet(out _hdriSky);
            ProgramManager.Instance.SceneVolume.profile.TryGet(out _colorAdjustments);
        }

        public void ToggleGui()
        {
            if (!gameObject.activeSelf)
            {
                MainGui.Instance.CloseWindows();
                TextureManager.Instance.FixSize();
                TextureManager.Instance.SetFullMaterialAndUpdate();
                Initialize();
                gameObject.SetActive(true);
                TestRotator.Reset();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void GetValues(ProjectObject projectObject)
        {
            InitializeSettings();
            projectObject.MaterialSettings = _materialSettings;
        }

        public void SetValues(ProjectObject projectObject)
        {
            if (projectObject.MaterialSettings != null)
            {
                _materialSettings = projectObject.MaterialSettings;
            }
            else
            {
                _settingsInitialized = false;
                InitializeSettings();
            }
        }

        private void OnEnable()
        {
            _settingsInitialized = false;
            InitializeSettings();
        }

        private void InitializeSettings()
        {
            _thisMaterial = TextureManager.Instance.FullMaterialInstance;
            if (_settingsInitialized) return;
            Logger.Log("Initializing MaterialSettings");
            _materialSettings = new MaterialSettings();
            _myColorTexture = TextureManager.Instance.GetStandardTexture(1, 1);
            _materialSettings.DisplacementStrength = _thisMaterial.GetFloat(DisplacementStrength);
            _materialSettings.NormalScale = _thisMaterial.GetFloat(NormalScaleId);
            _materialSettings.MetallicMultiplier = _thisMaterial.GetFloat(MetallicMultiplierId);
            _materialSettings.SmoothnessMultiplier = _thisMaterial.GetFloat(SmoothnessMultiplierId);
            _materialSettings.AoMultiplier = _thisMaterial.GetFloat(AoMultiplierId);
            _materialSettings.TexTilingX = _thisMaterial.GetTextureScale(DiffuseMap).x;
            _materialSettings.TexTilingY = _thisMaterial.GetTextureScale(DiffuseMap).y;
            _materialSettings.TexOffsetX = _thisMaterial.GetTextureOffset(DiffuseMap).x;
            _materialSettings.TexOffsetY = _thisMaterial.GetTextureOffset(DiffuseMap).y;

            _settingsInitialized = true;
        }

        private void Update()
        {
            if (ProgramManager.Instance.ApplicationIsQuitting) return;
            if (_divisorCount > 0)
            {
                _divisorCount--;
                return;
            }

            _divisorCount = UpdateDivisor;

            if (!_settingsInitialized) InitializeSettings();

            if (!_thisMaterial || _materialSettings == null) return;

            _thisMaterial.SetFloat(NormalScaleId, _materialSettings.NormalScale);
            _thisMaterial.SetFloat(MetallicMultiplierId, _materialSettings.MetallicMultiplier);
            _thisMaterial.SetFloat(SmoothnessMultiplierId, _materialSettings.SmoothnessMultiplier);
            _thisMaterial.SetFloat(AoMultiplierId, _materialSettings.AoMultiplier);

            var color = new Color(_materialSettings.LightR, _materialSettings.LightG, _materialSettings.LightB);
            _colorAdjustments.colorFilter.value = color;
            _hdriSky.exposure.value = _materialSettings.LightExposure;

            if (TestObjectParent.activeSelf != _planeShown) TestObjectParent.SetActive(_planeShown);
            if (TestObjectCube.activeSelf != _cubeShown) TestObjectCube.SetActive(_cubeShown);
            if (TestObjectCylinder.activeSelf != _cylinderShown) TestObjectCylinder.SetActive(_cylinderShown);
            if (TestObjectSphere.activeSelf != _sphereShown) TestObjectSphere.SetActive(_sphereShown);

            TextureManager.Instance.SetDisplacement(_materialSettings.DisplacementStrength);

            TextureManager.Instance.SetUvScale(new Vector2(_materialSettings.TexTilingX, _materialSettings.TexTilingY));
            TextureManager.Instance.SetUvOffset(new Vector2(_materialSettings.TexOffsetX,
                _materialSettings.TexOffsetY));


//            
        }

        private void ChooseLightColor(int posX, int posY)
        {
            _materialSettings.LightR =
                GUI.VerticalSlider(new Rect(posX + 10, posY + 5, 30, 90), _materialSettings.LightR, 1.0f, 0.0f);
            _materialSettings.LightG =
                GUI.VerticalSlider(new Rect(posX + 40, posY + 5, 30, 90), _materialSettings.LightG, 1.0f, 0.0f);
            _materialSettings.LightB =
                GUI.VerticalSlider(new Rect(posX + 70, posY + 5, 30, 90), _materialSettings.LightB, 1.0f, 0.0f);
            _materialSettings.LightExposure =
                GUI.VerticalSlider(new Rect(posX + 120, posY + 5, 30, 90), _materialSettings.LightExposure, 30.0f,
                    0.0f);

            GUI.Label(new Rect(posX + 10, posY + 95, 30, 25), "R");
            GUI.Label(new Rect(posX + 40, posY + 95, 30, 25), "G");
            GUI.Label(new Rect(posX + 70, posY + 95, 30, 25), "B");
            GUI.Label(new Rect(posX + 100, posY + 95, 100, 25), "Intensity");

            SetColorTexture();

            GUI.DrawTexture(new Rect(posX + 170, posY + 5, 90, 90), _myColorTexture);
        }

        private void SetColorTexture()
        {
            var colorArray = new Color[1];
            colorArray[0] = new Color(_materialSettings.LightR, _materialSettings.LightG, _materialSettings.LightB,
                1.0f);

            _myColorTexture.SetPixels(colorArray);
            _myColorTexture.Apply(false);
        }

        private void DoMyWindow(int windowId)
        {
            const int offsetX = 10;
            var offsetY = 20;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Metallic Multiplier",
                _materialSettings.MetallicMultiplier,
                out _materialSettings.MetallicMultiplier, 0.0f, 3.0f);
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Normal Scale", _materialSettings.NormalScale,
                out _materialSettings.NormalScale, 0.0f, 3.0f);
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Smoothness Multiplier",
                _materialSettings.SmoothnessMultiplier,
                out _materialSettings.SmoothnessMultiplier, 0.0f, 3.0f);
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Ao Multiplier", _materialSettings.AoMultiplier,
                out _materialSettings.AoMultiplier, 0.0f, 3.0f);
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Displacement Amplitude",
                _materialSettings.DisplacementStrength, out _materialSettings.DisplacementStrength, 0.0f, 5.0f);
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Texture Tiling (X,Y)", _materialSettings.TexTilingX,
                out _materialSettings.TexTilingX, 0.1f, 5.0f);
            offsetY += 25;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), _materialSettings.TexTilingY,
                out _materialSettings.TexTilingY, 0.1f, 5.0f);
            offsetY += 40;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Texture Offset (X,Y)", _materialSettings.TexOffsetX,
                out _materialSettings.TexOffsetX, -1.0f, 1.0f);
            offsetY += 25;

            GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), _materialSettings.TexOffsetY,
                out _materialSettings.TexOffsetY, -1.0f, 1.0f);
            offsetY += 40;

            GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Light Color");
            ChooseLightColor(offsetX, offsetY + 20);
            offsetY += 140;

            if (GUI.Button(new Rect(offsetX, offsetY, 60, 30), "Plane"))
            {
                _planeShown = true;
                _cubeShown = false;
                _cylinderShown = false;
                _sphereShown = false;
                Shader.DisableKeyword("TOP_PROJECTION");
            }

            if (GUI.Button(new Rect(offsetX + 70, offsetY, 60, 30), "Cube"))
            {
                _planeShown = false;
                _cubeShown = true;
                _cylinderShown = false;
                _sphereShown = false;
                Shader.EnableKeyword("TOP_PROJECTION");
            }

            if (GUI.Button(new Rect(offsetX + 140, offsetY, 70, 30), "Cylinder"))
            {
                _planeShown = false;
                _cubeShown = false;
                _cylinderShown = true;
                _sphereShown = false;
                Shader.EnableKeyword("TOP_PROJECTION");
            }

            if (GUI.Button(new Rect(offsetX + 220, offsetY, 60, 30), "Sphere"))
            {
                _planeShown = false;
                _cubeShown = false;
                _cylinderShown = false;
                _sphereShown = true;
                Shader.EnableKeyword("TOP_PROJECTION");
            }

            GUI.DragWindow();
        }

        private void OnGUI()
        {
            if (Hide) return;
            MainGui.MakeScaledWindow(_windowRect, _windowId, DoMyWindow, "Full Material", 0.9f);
        }

        public void Initialize()
        {
            InitializeSettings();
            TestObject.GetComponent<Renderer>().sharedMaterial = _thisMaterial;
            TestObjectCube.GetComponent<Renderer>().sharedMaterial = _thisMaterial;
            TestObjectCylinder.GetComponent<Renderer>().sharedMaterial = _thisMaterial;
            TestObjectSphere.GetComponent<Renderer>().sharedMaterial = _thisMaterial;
        }
    }
}