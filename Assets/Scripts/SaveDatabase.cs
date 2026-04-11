// GameSaveSystem.cs
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class SaveDatabase
{
    public int activeProfileId = -1;
    public List<SaveProfile> profiles = new List<SaveProfile>();
}

[Serializable]
public class SaveProfile
{
    public int profileId;
    public string profileName;
    public string createdAtUtc;
    public List<LevelSaveData> completedLevels = new List<LevelSaveData>();
}

[Serializable]
public class LevelSaveData
{
    public int levelIndex;
    public string levelName;
    public float bestTime;
    public int bestSteps;
    public int stars;
}

public static class GameSaveSystem
{
    public const int MaxProfiles = 3;

    private static SaveDatabase cache;

    private static string SavePath => Path.Combine(Application.persistentDataPath, "save_profiles.json");

    private static SaveDatabase Data
    {
        get
        {
            if (cache == null)
                cache = LoadDatabase();

            return cache;
        }
    }

    private static SaveDatabase LoadDatabase()
    {
        if (!File.Exists(SavePath))
            return new SaveDatabase();

        try
        {
            string json = File.ReadAllText(SavePath);
            if (string.IsNullOrWhiteSpace(json))
                return new SaveDatabase();

            SaveDatabase db = JsonUtility.FromJson<SaveDatabase>(json);
            return db ?? new SaveDatabase();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Could not load save file: {e.Message}");
            return new SaveDatabase();
        }
    }

    private static void SaveDatabaseToDisk()
    {
        try
        {
            string json = JsonUtility.ToJson(Data, true);
            File.WriteAllText(SavePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"Could not save file: {e.Message}");
        }
    }

    public static List<SaveProfile> GetProfiles()
    {
        return new List<SaveProfile>(Data.profiles);
    }

    public static int GetActiveProfileId()
    {
        return Data.activeProfileId;
    }

    public static bool HasProfile(int profileId)
    {
        return GetProfile(profileId) != null;
    }

    public static SaveProfile GetProfile(int profileId)
    {
        return Data.profiles.Find(p => p.profileId == profileId);
    }

    public static int CreateProfile(string profileName)
    {
        if (Data.profiles.Count >= MaxProfiles)
        {
            Debug.LogWarning("Maximum number of profiles reached.");
            return -1;
        }

        int newId = 0;
        while (HasProfile(newId))
            newId++;

        SaveProfile profile = new SaveProfile
        {
            profileId = newId,
            profileName = string.IsNullOrWhiteSpace(profileName) ? $"Profile {newId + 1}" : profileName,
            createdAtUtc = DateTime.UtcNow.ToString("o")
        };

        Data.profiles.Add(profile);

        if (Data.activeProfileId == -1)
            Data.activeProfileId = newId;

        SaveDatabaseToDisk();
        return newId;
    }

    public static bool DeleteProfile(int profileId)
    {
        SaveProfile profile = GetProfile(profileId);
        if (profile == null)
            return false;

        Data.profiles.Remove(profile);

        if (Data.activeProfileId == profileId)
            Data.activeProfileId = Data.profiles.Count > 0 ? Data.profiles[0].profileId : -1;

        SaveDatabaseToDisk();
        return true;
    }

    public static bool SetActiveProfile(int profileId)
    {
        if (!HasProfile(profileId))
            return false;

        Data.activeProfileId = profileId;
        SaveDatabaseToDisk();
        return true;
    }

    public static bool RecordLevelPassed(int levelIndex, LevelData level, float timeSeconds, int steps)
    {
        SaveProfile profile = GetProfile(Data.activeProfileId);
        if (profile == null)
        {
            Debug.LogWarning("No active profile selected. Level result was not saved.");
            return false;
        }

        int stars = CalculateStars(level, timeSeconds, steps);

        LevelSaveData existing = profile.completedLevels.Find(l => l.levelIndex == levelIndex);

        if (existing == null)
        {
            profile.completedLevels.Add(new LevelSaveData
            {
                levelIndex = levelIndex,
                levelName = level.levelName,
                bestTime = timeSeconds,
                bestSteps = steps,
                stars = stars
            });
        }
        else
        {
            bool betterRun =
                stars > existing.stars ||
                (stars == existing.stars && steps < existing.bestSteps) ||
                (stars == existing.stars && steps == existing.bestSteps && timeSeconds < existing.bestTime);

            if (betterRun)
            {
                existing.levelName = level.levelName;
                existing.bestTime = timeSeconds;
                existing.bestSteps = steps;
                existing.stars = stars;
            }
        }

        SaveDatabaseToDisk();
        return true;
    }

    public static LevelSaveData GetLevelData(int profileId, int levelIndex)
    {
        SaveProfile profile = GetProfile(profileId);
        if (profile == null)
            return null;

        return profile.completedLevels.Find(l => l.levelIndex == levelIndex);
    }

    public static int CalculateStars(LevelData level, float timeSeconds, int steps)
    {
        int stars = 1;

        if (steps <= level.minSteps)
            stars++;

        if (MathF.Floor(timeSeconds) <= level.minTime)
            stars++;

        return stars;
    }

    public static void DeleteAllSaveData()
    {
        cache = new SaveDatabase();

        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }

    public static int GetFirstUnlockedUnpassedLevelIndex(int profileId, int totalLevelCount)
    {
        SaveProfile profile = GetProfile(profileId);
        if (profile == null)
            return totalLevelCount > 0 ? 0 : -1;

        for (int i = 0; i < totalLevelCount; i++)
        {
            bool allPreviousPassed = true;

            for (int previous = 0; previous < i; previous++)
            {
                if (profile.completedLevels.Find(l => l.levelIndex == previous) == null)
                {
                    allPreviousPassed = false;
                    break;
                }
            }

            bool currentPassed = profile.completedLevels.Find(l => l.levelIndex == i) != null;

            if (allPreviousPassed && !currentPassed)
                return i;
        }

        return -1;
    }
}