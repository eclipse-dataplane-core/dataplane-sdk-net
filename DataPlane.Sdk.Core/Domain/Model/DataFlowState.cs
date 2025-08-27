namespace DataPlane.Sdk.Core.Domain.Model;

public enum DataFlowState
{
    /// <summary>
    ///     The state machine has been initialized.
    /// </summary>
    Initialized = 100,

    /// <summary>
    ///     A consumer or provider data plane are in the process of preparing to receive or send data using a wire protocol.
    ///     This process may involve provisioning resources such as access tokens or preparing data. Used in asynchronous
    ///     requests, when some resources have to be provisioned first.
    /// </summary>
    Preparing = 200,

    /// <summary>
    ///     Used in asynchronous requests, when resource provisioning has finished, or in synchronous requests, after the
    ///     request was processed. The consumer or provider is ready to receive or send data.
    /// </summary>
    Prepared = 300,

    /// <summary>
    ///     The consumer or provider is starting the wire protocol. Entered after an asynchronous Start message was received
    /// </summary>
    Starting = 400,

    /// <summary>
    ///     The consumer or provider has started sending data using the wire protocol. Entered after the dataplane has finished
    ///     its preparations and is ready to start processing, or directly after a
    ///     synchronous Start message was received.
    /// </summary>
    Started = 500,

    /// <summary>
    ///     A data transfer is temporarily paused and can be resumed.
    /// </summary>
    Suspended = 600,

    /// <summary>
    ///     A data transfer has been completed normally and cannot be resumed. This is a terminal state.
    /// </summary>
    Completed = 700,

    /// <summary>
    ///     A data transfer has terminated before completion, has failed or experienced an error and cannot be resumed. This is
    ///     a terminal state.
    /// </summary>
    Terminated = 800,

    #region Deprecated states

    Provisioning = 25,
    Provisioned = 50,
    Received = 60,
    Notified = 70,
    Deprovisioning = 80,

    #endregion
}