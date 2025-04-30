namespace BackgroundJobCodingChallenge.JobEngine.WorkerObjects.Workitem;

/// <summary>
/// IUserInfo is an interface that defines the properties of a user info object.
/// </summary>
public interface IUserInfo : IWorkItem
{
    /// <summary>
    /// UserName is the name of the user.
    /// </summary>
    string UserName { get; }

    /// <summary>
    /// UserId is the unique identifier for the user.
    /// </summary>
    Guid UserId { get; }

    /// <summary>
    /// UserEmail is the email address of the user.
    /// </summary>
    string UserEmail { get; }

    /// <summary>
    /// UserPhone is the phone number of the user.
    /// </summary>
    string UserPhone { get; }

}
