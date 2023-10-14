using System.Collections.ObjectModel;
using DynamicData;

namespace MuseDashModToolsUI.Contracts;

public interface IChartService
{
    /// <summary>
    ///     Download Chart
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    Task DownloadChart(Chart item);

    /// <summary>
    ///     Initialize Chart list
    /// </summary>
    /// <param name="sourceCache"></param>
    /// <param name="charts"></param>
    /// <returns></returns>
    Task InitializeChartList(SourceCache<Chart, string> sourceCache, ReadOnlyObservableCollection<Chart> charts);

    /// <summary>
    ///     Choose Chart folder
    /// </summary>
    /// <returns></returns>
    Task OnChooseChartFolder();
}