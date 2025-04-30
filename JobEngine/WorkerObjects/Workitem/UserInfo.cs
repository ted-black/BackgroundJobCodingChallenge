namespace BackgroundJobCodingChallenge.JobEngine.WorkerObjects.Workitem;

/// <summary>
/// UserInfo is a class that implements the IUserInfo interface.
/// </summary>
public class UserInfo : IUserInfo
{
    /// <summary>
    /// UserInfo is a class that implements the IUserInfo interface.
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="userId"></param>
    /// <param name="userEmail"></param>
    /// <param name="userPhone"></param>
    public UserInfo(string userName, Guid userId, string userEmail, string userPhone)
    {
        UserName = userName;
        UserId = userId;
        UserEmail = userEmail;
        UserPhone = userPhone;
    }

    /// <inheritdoc cref="IWorkItem.Id"/>
    public int Id { get; set; } = 0;

    /// <inheritdoc cref="IUserInfo.UserName"/>
    public string UserName { get; private set; } = string.Empty;

    /// <inheritdoc cref="IUserInfo.UserId"/>
    public Guid UserId  { get; private set; }

    /// <inheritdoc cref="IUserInfo.UserEmail"/>
    public string UserEmail  { get; private set; } = string.Empty;

    /// <inheritdoc cref="IUserInfo.UserPhone"/>
    public string UserPhone  { get; private set; } = string.Empty;
}
