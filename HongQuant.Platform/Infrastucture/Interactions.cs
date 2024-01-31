using ReactiveUI;

namespace HongQuant.Platform.Infrastucture;
public class Interactions
{
    public static readonly Interaction<string, bool> HongSettingConfirm = new();
    public static readonly Interaction<string, bool> HongSettingError = new();
    public static readonly Interaction<string, string> HongSettingSaveFileDialog = new();
    public static readonly Interaction<string, string> HongSettingOpenFileDialog = new();


    public static readonly Interaction<string, string> YiTouOpenFileDialog = new();
}
