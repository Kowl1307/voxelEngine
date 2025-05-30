using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Voxel_Engine.WorldGen.Noise;
using Random = UnityEngine.Random;

namespace Voxel_Engine.NoiseVisualizer
{
    public class NoiseVisualizerUI : MonoBehaviour
    {
        [SerializeField] 
        private TMP_Dropdown _noiseSelectionDropdown;
        
        private INoiseProvider _noiseProvider;
        
        [SerializeField] private TMP_InputField _seedInputField;
        private int _seed = 0;

        [SerializeField] private TMP_InputField _sizeInputField;
        private int _size;
        
        [SerializeField] private RawImage _upperNoiseImage;
        [SerializeField] private RawImage _lowerNoiseImage;

        [SerializeField] private TMP_InputField _zoomInputField;
        [SerializeField] private TMP_InputField _octavesInputField;
        [SerializeField] private TMP_InputField _redistributionInputField;
        [SerializeField] private TMP_InputField _ExponentField;
        
        
        [SerializeField] private TMP_InputField _upperOffsetXInputField;
        [SerializeField] private TMP_InputField _upperOffsetYInputField;
        [SerializeField] private TMP_InputField _lowerOffsetXInputField;
        [SerializeField] private TMP_InputField _lowerOffsetYInputField;
        
        private NoiseSettings upperNoiseSettings;
        private NoiseSettings lowerNoiseSettings;
        

        private void Start()
        {
            upperNoiseSettings = ScriptableObject.CreateInstance<NoiseSettings>();
            lowerNoiseSettings = ScriptableObject.CreateInstance<NoiseSettings>();
            
            foreach (var inputField in new[]
                     {
                         _seedInputField, _sizeInputField,
                         _zoomInputField, _octavesInputField, _redistributionInputField, _ExponentField,
                         _upperOffsetXInputField, _upperOffsetYInputField, 
                         _lowerOffsetXInputField, _lowerOffsetYInputField,
                     })
            {
                inputField.onEndEdit.AddListener(delegate { GenerateNoiseImages(); });
            }

            _zoomInputField.text = upperNoiseSettings.NoiseZoom.ToString();
            _octavesInputField.text = upperNoiseSettings.Octaves.ToString();
            _redistributionInputField.text = upperNoiseSettings.RedistributionModifier.ToString();
            _ExponentField.text = upperNoiseSettings.Exponent.ToString();

            _upperOffsetXInputField.text = "0";
            _upperOffsetYInputField.text = "0";
            _lowerOffsetXInputField.text = "0";
            _lowerOffsetYInputField.text = "0";
            
            _noiseSelectionDropdown.options = new List<TMP_Dropdown.OptionData>(){new("Domain Warping"), new("Biome Refining")};
            _noiseSelectionDropdown.onValueChanged.AddListener(OnDropdownValueChanged);

            _noiseSelectionDropdown.value = 0;

            _sizeInputField.text = "200";
            OnDropdownValueChanged(_noiseSelectionDropdown.value);
            RerollSeed();
            GenerateNoiseImages();
        }

        private void OnDropdownValueChanged(int value)
        {
            _noiseProvider = value switch
            {
                0 => new DomainWarpingProviderWrapper(),
                1 => new BiomeRefiningProvider(),
                _ => _noiseProvider
            };
        }

        public void RerollSeed()
        {
            _seed = Random.Range(0, 100000);
            _seedInputField.text = _seed.ToString();

            upperNoiseSettings.Seed = new Vector2Int(_seed, _seed);
            lowerNoiseSettings.Seed = new Vector2Int(_seed, _seed);
        }

        public void GenerateNoiseImages()
        {
            LoadNoiseSettingsFromFields();

            var width = _size;
            var height = _size;
            
            Debug.Log("Started Generating Noise Images");
            var upperNoise = _noiseProvider.GetNoiseValues(width, height, upperNoiseSettings);
            var lowerNoise = _noiseProvider.GetNoiseValues(width, height, lowerNoiseSettings);

            var upperTexture = new Texture2D(width,height,TextureFormat.ARGB32, false)
            {
                filterMode = FilterMode.Point
            };
            var lowerTexture = new Texture2D(width,height,TextureFormat.ARGB32, false)
            {
                filterMode = FilterMode.Point
            };

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    upperTexture.SetPixel(x,y, new Color(upperNoise[x,y], upperNoise[x,y], upperNoise[x,y]));
                    lowerTexture.SetPixel(x,y, new Color(lowerNoise[x,y], lowerNoise[x,y], lowerNoise[x,y]));
                }
            }
            
            upperTexture.Apply();
            lowerTexture.Apply();
            
            _upperNoiseImage.texture = upperTexture;
            _lowerNoiseImage.texture = lowerTexture;
        }

        private void LoadNoiseSettingsFromFields()
        {
            _seed = Convert.ToInt32(_seedInputField.text);
            _size = Convert.ToInt32(_sizeInputField.text);
            
            upperNoiseSettings.NoiseZoom = Convert.ToSingle(_zoomInputField.text);
            upperNoiseSettings.Octaves = Convert.ToInt32(_octavesInputField.text);
            upperNoiseSettings.RedistributionModifier = Convert.ToSingle(_redistributionInputField.text);
            upperNoiseSettings.Exponent = Convert.ToSingle(_ExponentField.text);
            
            lowerNoiseSettings.NoiseZoom = upperNoiseSettings.NoiseZoom;
            lowerNoiseSettings.Octaves = upperNoiseSettings.Octaves;
            lowerNoiseSettings.RedistributionModifier = upperNoiseSettings.RedistributionModifier;
            lowerNoiseSettings.Exponent = upperNoiseSettings.Exponent;

            upperNoiseSettings.Offset.x = Convert.ToInt32(_upperOffsetXInputField.text);
            upperNoiseSettings.Offset.y = Convert.ToInt32(_upperOffsetYInputField.text);
            lowerNoiseSettings.Offset.x = Convert.ToInt32(_lowerOffsetXInputField.text);
            lowerNoiseSettings.Offset.y = Convert.ToInt32(_lowerOffsetYInputField.text);

            upperNoiseSettings.Seed = new Vector2Int(_seed, _seed);
            lowerNoiseSettings.Seed = new Vector2Int(_seed, _seed);
        }
    }
}
