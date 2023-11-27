
using CommunityToolkit.Mvvm.ComponentModel;

namespace ParamLimitsTest
{
    public class MainWindowViewModel: ObservableObject
    {
        public TestParamsLimitViewModel TestParamsLimit { get; set; }

        public MainWindowViewModel() 
        {
            TestParamsLimit = new TestParamsLimitViewModel(null);
        }
    }
}
