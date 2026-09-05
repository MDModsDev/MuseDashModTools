namespace Euterpe.Tests.Models.Charts;

[Category("ChartFilesTests")]
[TestSubject(typeof(ChartFiles))]
public sealed class ChartFilesTest
{
    [Test]
    [Arguments("manifest.epk")]
    [Arguments("music.ogg")]
    [Arguments("demo.ogg")]
    [Arguments("video.mp4")]
    [Arguments("cover.webp")]
    [Arguments("map1.bms")]
    [Arguments("map4.bms")]
    [Arguments("map3.talk")]
    public async Task IsChartFile_KnownChartFile_ReturnsTrue(string fileName) =>
        await Assert.That(ChartFiles.IsChartFile(fileName)).IsTrue();

    [Test]
    [Arguments("thumbs.db")]
    [Arguments("music.ogg.tmp")]
    [Arguments("notes.txt")]
    [Arguments("song.mp3")]
    [Arguments("map.bms")]
    [Arguments("mapx.bms")]
    [Arguments("covers.webp")]
    public async Task IsChartFile_ForeignFile_ReturnsFalse(string fileName) =>
        await Assert.That(ChartFiles.IsChartFile(fileName)).IsFalse();
}
