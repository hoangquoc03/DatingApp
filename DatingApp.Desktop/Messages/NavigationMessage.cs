using CommunityToolkit.Mvvm.Messaging.Messages;

namespace DatingApp.Desktop.Messages;

public class NavigationMessage : ValueChangedMessage<object>
{
    public NavigationMessage(object viewModel) : base(viewModel)
    {
    }
}
