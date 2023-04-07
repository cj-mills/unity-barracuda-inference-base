using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Barracuda;
using System;

namespace CJM.BarracudaInferenceToolkit
{
    public class MultiClassImageClassifier : BarracudaModelRunner
    {
        [Tooltip("Target output layer index")]
        [SerializeField] private int outputLayerIndex = 0;
        [Tooltip("JSON file with class labels")]
        [SerializeField] private TextAsset classLabels;

        // Indicates if the system supports asynchronous GPU readback
        private bool supportsAsyncGPUReadback = false;

        private const string TransposeLayer = "transpose";
        private string SoftmaxLayer = "softmaxLayer";
        private string outputLayer;

        // Helper class for deserializing class labels from the JSON file
        private class ClassLabels { public string[] classes; }

        private string[] classes;

        // Texture formats for output processing
        private TextureFormat textureFormat = TextureFormat.RGBA32;
        private RenderTextureFormat renderTextureFormat = RenderTextureFormat.ARGB32;

        // Output textures for processing on CPU and GPU
        private Texture2D outputTextureCPU;
        private RenderTexture outputTextureGPU;

        /// <summary>
        /// Initialize necessary components during the start of the script.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            CheckAsyncGPUReadbackSupport(); // Check if async GPU readback is supported
            LoadClassLabels(); // Load class labels from JSON file
            CreateOutputTextures(); // Initialize output texture
        }

        // Check if the system supports async GPU readback
        public bool CheckAsyncGPUReadbackSupport()
        {
            supportsAsyncGPUReadback = SystemInfo.supportsAsyncGPUReadback && supportsAsyncGPUReadback;
            return supportsAsyncGPUReadback;
        }

        /// <summary>
        /// Load the model and prepare it for execution by applying softmax to the output layer.
        /// </summary>
        protected override void LoadAndPrepareModel()
        {
            // Load and prepare the model with the base implementation
            base.LoadAndPrepareModel();

            outputLayer = modelBuilder.model.outputs[0];

            // Set worker type for WebGL
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                workerType = WorkerFactory.Type.PixelShader;
            }

            // Check if the last layer is a Softmax layer
            Layer lastLayer = modelBuilder.model.layers[modelBuilder.model.layers.Count - 1];
            bool lastLayerIsSoftmax = lastLayer.activation == Layer.Activation.Softmax;

            // Add the Softmax layer if the last layer is not already a Softmax layer
            if (!lastLayerIsSoftmax)
            {
                // Add the Softmax layer
                modelBuilder.Softmax(SoftmaxLayer, outputLayer);
                outputLayer = SoftmaxLayer;
            }

            // Apply transpose operation on the output layer
            modelBuilder.Transpose(TransposeLayer, outputLayer, new[] { 0, 1, 3, 2 });
            outputLayer = TransposeLayer;
        }

        /// <summary>
        /// Initialize the inference engine and check if the model is using a Compute Shader backend.
        /// </summary>
        protected override void InitializeEngine()
        {
            base.InitializeEngine();

            // Check if async GPU readback is supported by the engine
            supportsAsyncGPUReadback = engine.Summary().Contains("Unity.Barracuda.ComputeVarsWithSharedModel");
        }

        /// <summary>
        /// Load the class labels from the provided JSON file.
        /// </summary>
        private void LoadClassLabels()
        {
            if (IsClassLabelsJsonNullOrEmpty())
            {
                Debug.LogError("Class labels JSON is null or empty.");
                return;
            }

            ClassLabels classLabelsObj = DeserializeClassLabels(classLabels.text);
            UpdateClassLabels(classLabelsObj);
        }

        /// <summary>
        /// Check if the provided class labels JSON file is null or empty.
        /// </summary>
        /// <returns>True if the file is null or empty, otherwise false.</returns>
        private bool IsClassLabelsJsonNullOrEmpty()
        {
            return classLabels == null || string.IsNullOrWhiteSpace(classLabels.text);
        }

        /// <summary>
        /// Deserialize the provided class labels JSON string to a ClassLabels object.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>A deserialized ClassLabels object.</returns>
        private ClassLabels DeserializeClassLabels(string json)
        {
            try
            {
                return JsonUtility.FromJson<ClassLabels>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to deserialize class labels JSON: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Update the classes array with the provided ClassLabels object.
        /// </summary>
        /// <param name="classLabelsObj">The ClassLabels object containing the class labels.</param>
        private void UpdateClassLabels(ClassLabels classLabelsObj)
        {
            if (classLabelsObj == null)
            {
                return;
            }

            classes = classLabelsObj.classes;
        }

        /// <summary>
        /// Create the output textures that will store the model output.
        /// </summary>
        private void CreateOutputTextures()
        {
            outputTextureCPU = new Texture2D(classes.Length, 1, textureFormat, false);
            outputTextureGPU = RenderTexture.GetTemporary(classes.Length, 1, 0, renderTextureFormat);
        }

        /// <summary>
        /// Execute the model on the provided input texture and return the output array.
        /// </summary>
        /// <param name="inputTexture">The input texture for the model.</param>
        public void ExecuteModel(RenderTexture inputTexture)
        {
            using (Tensor input = new Tensor(inputTexture, channels: 3))
            {
                base.ExecuteModel(input);
            }
        }


        /// <summary>
        /// Copy the model output to a float array.
        /// </summary>
        public float[] CopyOutputToArray()
        {
            using (Tensor output = engine.PeekOutput(outputLayer))
            {
                if (workerType == WorkerFactory.Type.PixelShader)
                {
                    Resources.UnloadUnusedAssets();
                }
                return output.data.Download(output.shape);
            }
        }


        /// <summary>
        /// Copy the model output to a texture.
        /// </summary>
        public void CopyOutputToTexture()
        {
            using (Tensor output = engine.PeekOutput(outputLayer))
            {
                // Debug.Log(output.shape);
                // Tensor reshapedOutput = output.Reshape(new TensorShape(1, classes.Length, 1, 1));
                output.ToRenderTexture(outputTextureGPU);
            }
        }


        /// <summary>
        /// Copy the model output using async GPU readback. If not supported, defaults to synchronous readback.
        /// </summary>
        public float[] CopyOutputWithAsyncReadback()
        {
            if (!supportsAsyncGPUReadback)
            {
                Debug.Log("Async GPU Readback not supported. Defaulting to synchronous readback");
                return CopyOutputToArray();
            }

            CopyOutputToTexture();

            AsyncGPUReadback.Request(outputTextureGPU, 0, textureFormat, OnCompleteReadback);

            Color[] outputColors = outputTextureCPU.GetPixels();
            // return outputColors.Select(color => color.r).Reverse().ToArray();
            return outputColors.Select(color => color.r).ToArray();
        }


        /// <summary>
        /// Get the class name corresponding to the provided class index.
        /// </summary>
        /// <param name="classIndex">The index of the class to retrieve.</param>
        /// <returns>The class name corresponding to the class index.</returns>
        public string GetClassName(int classIndex)
        {
            return classes[classIndex];
        }

        /// <summary>
        /// Callback method for handling the completion of async GPU readback.
        /// </summary>
        /// <param name="request">The async GPU readback request.</param>
        private void OnCompleteReadback(AsyncGPUReadbackRequest request)
        {
            if (request.hasError)
            {
                Debug.Log("GPU readback error detected.");
                return;
            }

            if (outputTextureCPU != null)
            {
                try
                {
                    // Load readback data into the output texture and apply changes
                    outputTextureCPU.LoadRawTextureData(request.GetData<uint>());
                    outputTextureCPU.Apply();
                }
                catch (UnityException ex)
                {
                    if (ex.Message.Contains("LoadRawTextureData: not enough data provided (will result in overread)."))
                    {
                        Debug.Log("Updating input data size to match the texture size.");
                    }
                    else
                    {
                        Debug.LogError($"Unexpected UnityException: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Clean up resources when the script is disabled.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
            // Release the temporary render texture
            RenderTexture.ReleaseTemporary(outputTextureGPU);
        }
    }
}
