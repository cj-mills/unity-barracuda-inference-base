using UnityEngine;
using Unity.Barracuda;
using System.Collections.Generic;

namespace BarracudaInferenceToolkit
{
    public abstract class BarracudaModelRunner : MonoBehaviour
    {
        [Header("Model Assets")]
        [SerializeField] protected NNModel model;
        [Tooltip("Option to order tensor data channels first (EXPERIMENTAL)")]
        [SerializeField] private bool useNCHW = true;
        [Tooltip("Execution backend for the model")]
        [SerializeField] protected WorkerFactory.Type workerType = WorkerFactory.Type.Auto;

        protected ModelBuilder modelBuilder;
        protected IWorker engine;

        protected virtual void Start()
        {
            LoadAndPrepareModel();
            InitializeEngine();
        }

        /// <summary>
        /// Load and prepare the model for execution.
        /// Override this method to apply custom modifications to the model.
        /// </summary>
        protected virtual void LoadAndPrepareModel()
        {
            Model runtimeModel = ModelLoader.Load(model);
            modelBuilder = new ModelBuilder(runtimeModel);
        }

        /// <summary>
        /// Initialize the worker for executing the model with the specified backend and channel order.
        /// </summary>
        /// <param name="model">The target model representation.</param>
        /// <param name="workerType">The target compute backend.</param>
        /// <param name="useNCHW">The channel order for the compute backend (default is true).</param>
        /// <returns>An initialized worker instance.</returns>
        protected IWorker InitializeWorker(Model model, WorkerFactory.Type workerType, bool useNCHW = true)
        {
            // Validate worker type
            workerType = WorkerFactory.ValidateType(workerType);

            // Set channel order if required
            if (useNCHW)
            {
                ComputeInfo.channelsOrder = ComputeInfo.ChannelsOrder.NCHW;
            }

            // Create and return the worker instance
            return WorkerFactory.CreateWorker(workerType, model);
        }

        /// <summary>
        /// Initialize the inference engine.
        /// </summary>
        protected virtual void InitializeEngine()
        {
            engine = WorkerFactory.CreateWorker(workerType, modelBuilder.model);
            engine = InitializeWorker(modelBuilder.model, workerType, useNCHW);
        }

        /// <summary>
        /// Execute the model with the given input Tensor.
        /// Override this method to implement custom input and output processing.
        /// </summary>
        /// <param name="input">The input Tensor for the model.</param>
        public virtual void ExecuteModel(Tensor input)
        {
            engine.Execute(input);
        }

        /// <summary>
        /// Execute the model with the given input Tensor.
        /// Override this method to implement custom input and output processing.
        /// </summary>
        /// <param name="input">The input Tensor for the model.</param>
        public virtual void ExecuteModel(IDictionary<string, Tensor> inputs)
        {
            engine.Execute(inputs);
        }

        /// <summary>
        /// Clean up resources when the component is disabled.
        /// </summary>
        protected virtual void OnDisable()
        {
            engine.Dispose();
        }
    }
}
