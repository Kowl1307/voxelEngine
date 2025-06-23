using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Voxel_Engine.WorldGen.BiomeSelectors.BiomeRefining;
using Voxel_Engine.WorldGen.Noise;
using Random = UnityEngine.Random;

namespace Voxel_Engine.BiomeRefiningVisualizer
{
    public class BiomeRefiningVisualizerUI : MonoBehaviour
    {
        [SerializeField] 
        private TMP_Dropdown _noiseSelectionDropdown;
        
        private IBiomeRefiningNoiseProvider _noiseProvider;
        
        [SerializeField] private TMP_InputField _seedInputField;
        private int _seed = 0;

        [SerializeField] private TMP_InputField _sizeInputField;
        private int _size;

        [SerializeField] private TMP_Text _historyText;
        private int _historyLevel;
        
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
        
        private NoiseSettings _upperNoiseSettings;
        private NoiseSettings _lowerNoiseSettings;
        private BiomeRefiningHistory _upperHistory;
        private BiomeRefiningHistory _lowerHistory;
        private Color[,] _upperNoise;
        private Color[,] _lowerNoise;


        private void Start()
        {
            _upperNoiseSettings = ScriptableObject.CreateInstance<NoiseSettings>();
            _lowerNoiseSettings = ScriptableObject.CreateInstance<NoiseSettings>();
            
            foreach (var inputField in new[]
                     {
                         _seedInputField, _sizeInputField,
                         _zoomInputField, _octavesInputField, _redistributionInputField, _ExponentField,
                         _upperOffsetXInputField, _upperOffsetYInputField, 
                         _lowerOffsetXInputField, _lowerOffsetYInputField,
                     })
            {
                inputField.onEndEdit.AddListener(delegate { GenerateNoise(); GenerateNoiseImages();});
            }

            _zoomInputField.text = _upperNoiseSettings.NoiseZoom.ToString();
            _octavesInputField.text = _upperNoiseSettings.Octaves.ToString();
            _redistributionInputField.text = _upperNoiseSettings.RedistributionModifier.ToString();
            _ExponentField.text = _upperNoiseSettings.Exponent.ToString();

            _historyText.text = "0";

            _upperOffsetXInputField.text = "0";
            _upperOffsetYInputField.text = "0";
            _lowerOffsetXInputField.text = "0";
            _lowerOffsetYInputField.text = "0";
            
            _noiseSelectionDropdown.options = new List<TMP_Dropdown.OptionData>(){new("Biome Refining")};
            _noiseSelectionDropdown.onValueChanged.AddListener(OnDropdownValueChanged);

            _noiseSelectionDropdown.value = 0;

            _sizeInputField.text = "8";
            OnDropdownValueChanged(_noiseSelectionDropdown.value);
            RerollSeed();
            GenerateNoise();
            GenerateNoiseImages();
        }

        private void OnDropdownValueChanged(int value)
        {
            _noiseProvider = value switch
            {
                0 => new BiomeRefiningProvider(gameObject),
                _ => _noiseProvider
            };
        }

        public void RerollSeed()
        {
            _seed = Random.Range(0, 100000);
            _seedInputField.text = _seed.ToString();

            _upperNoiseSettings.Seed = new Vector2Int(_seed, _seed);
            _lowerNoiseSettings.Seed = new Vector2Int(_seed, _seed);
        }

        public void IncreaseHistoryDepth()
        {
            if (_historyLevel + 1 >= _upperHistory.GetHistoryDepth())
            {
                return;
            }
            
            _historyLevel++;
            _historyText.text = _historyLevel.ToString();
            GenerateNoiseImages();
        }
        
        public void DecreaseHistoryDepth()
        {
            if (_historyLevel <= 0) return;
            _historyLevel--;
            _historyText.text = _historyLevel.ToString();
            GenerateNoiseImages();
        }

        public void GenerateNoise()
        {
            LoadNoiseSettingsFromFields();
            
            _upperHistory = _noiseProvider.GetBiomeRefiningHistory(4096, _size, _size, _upperNoiseSettings);
            _lowerHistory = _noiseProvider.GetBiomeRefiningHistory(4096, _size, _size, _lowerNoiseSettings);
        }
        
        public void GenerateNoiseImages()
        {
            _upperNoise = _upperHistory.GetResolutionMapByIndex(_historyLevel).Map;
            _lowerNoise = _lowerHistory.GetResolutionMapByIndex(_historyLevel).Map;
            
            print(_upperNoise.GetLength(0));

            var width = _upperNoise.GetLength(0);
            var height = _upperNoise.GetLength(1);
            
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
                    upperTexture.SetPixel(x,y, _upperNoise[x,y]);
                    lowerTexture.SetPixel(x,y, _lowerNoise[x,y]);
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
            
            _upperNoiseSettings.NoiseZoom = Convert.ToSingle(_zoomInputField.text);
            _upperNoiseSettings.Octaves = Convert.ToInt32(_octavesInputField.text);
            _upperNoiseSettings.RedistributionModifier = Convert.ToSingle(_redistributionInputField.text);
            _upperNoiseSettings.Exponent = Convert.ToSingle(_ExponentField.text);
            
            _lowerNoiseSettings.NoiseZoom = _upperNoiseSettings.NoiseZoom;
            _lowerNoiseSettings.Octaves = _upperNoiseSettings.Octaves;
            _lowerNoiseSettings.RedistributionModifier = _upperNoiseSettings.RedistributionModifier;
            _lowerNoiseSettings.Exponent = _upperNoiseSettings.Exponent;

            _historyLevel = Convert.ToInt32(_historyText.text);

            _upperNoiseSettings.Offset.x = Convert.ToInt32(_upperOffsetXInputField.text);
            _upperNoiseSettings.Offset.y = Convert.ToInt32(_upperOffsetYInputField.text);
            _lowerNoiseSettings.Offset.x = Convert.ToInt32(_lowerOffsetXInputField.text);
            _lowerNoiseSettings.Offset.y = Convert.ToInt32(_lowerOffsetYInputField.text);

            _upperNoiseSettings.Seed = new Vector2Int(_seed, _seed);
            _lowerNoiseSettings.Seed = new Vector2Int(_seed, _seed);
        }
    }
}
