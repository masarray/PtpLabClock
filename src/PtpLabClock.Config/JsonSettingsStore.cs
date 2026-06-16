// SPDX-License-Identifier: Apache-2.0
using System.Text.Json;

namespace PtpLabClock.Config;

public sealed class JsonSettingsStore<T> where T : new()
{
    private readonly string _path;

    public JsonSettingsStore(string fileName)
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PtpLabClock");
        Directory.CreateDirectory(dir);
        _path = Path.Combine(dir, fileName);
    }

    public T Load()
    {
        if (!File.Exists(_path)) return new T();
        var json = File.ReadAllText(_path);
        return JsonSerializer.Deserialize<T>(json) ?? new T();
    }

    public void Save(T value)
    {
        var json = JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_path, json);
    }
}
