using CustomizePlus.Profiles.Data;
using Dalamud.Plugin;

namespace CustomizePlus.Core.Services;

public class FilenameService(IDalamudPluginInterface pi) : BaseFilePathProvider(pi)
{
    public new readonly string ConfigDirectory = pi.ConfigDirectory.FullName;
    public new readonly string ConfigFile = pi.ConfigFile.FullName;

    public readonly string ProfileDirectory = Path.Combine(pi.ConfigDirectory.FullName, "profiles");
    public readonly string LegacyProfileSortOrder = Path.Combine(pi.ConfigDirectory.FullName, "profile_sort_order.json");

    // New subfolder layout matching official Customize+ 2.x release.
    // Files are stored in profile_filesystem/ and template_filesystem/ subfolders.
    public readonly string ProfileFileSystemDirectory = Path.Combine(pi.ConfigDirectory.FullName, "profile_filesystem");
    public readonly string ProfileOrganization = Path.Combine(pi.ConfigDirectory.FullName, "profile_filesystem", "organization.json");
    public readonly string ProfileLockedNodes = Path.Combine(pi.ConfigDirectory.FullName, "profile_filesystem", "locked_nodes.json");
    public readonly string ProfileSelectedNodes = Path.Combine(pi.ConfigDirectory.FullName, "profile_filesystem", "selected_nodes.json");
    public readonly string ProfileExpandedFolders = Path.Combine(pi.ConfigDirectory.FullName, "profile_filesystem", "expanded_folders.json");

    public readonly string TemplateDirectory = Path.Combine(pi.ConfigDirectory.FullName, "templates");
    public readonly string LegacyTemplateSortOrder = Path.Combine(pi.ConfigDirectory.FullName, "template_sort_order.json");

    public readonly string TemplateFileSystemDirectory = Path.Combine(pi.ConfigDirectory.FullName, "template_filesystem");
    public readonly string TemplateOrganization = Path.Combine(pi.ConfigDirectory.FullName, "template_filesystem", "organization.json");
    public readonly string TemplateLockedNodes = Path.Combine(pi.ConfigDirectory.FullName, "template_filesystem", "locked_nodes.json");
    public readonly string TemplateSelectedNodes = Path.Combine(pi.ConfigDirectory.FullName, "template_filesystem", "selected_nodes.json");
    public readonly string TemplateExpandedFolders = Path.Combine(pi.ConfigDirectory.FullName, "template_filesystem", "expanded_folders.json");

    /// <summary>
    /// One-time migration helper. Moves any UI organization files from the old flat layout
    /// (used by earlier api15 builds) into the new subfolder layout used by official Customize+ 2.x.
    /// Safe to run on every startup - it only acts if old files exist and new ones don't.
    /// </summary>
    public void MigrateUiOrganizationFiles()
    {
        try
        {
            Directory.CreateDirectory(ProfileFileSystemDirectory);
            Directory.CreateDirectory(TemplateFileSystemDirectory);

            MoveIfNeeded(Path.Combine(ConfigDirectory, "profile_organization.json"), ProfileOrganization);
            MoveIfNeeded(Path.Combine(ConfigDirectory, "profile_locked_nodes.json"), ProfileLockedNodes);
            MoveIfNeeded(Path.Combine(ConfigDirectory, "profile_selected_nodes.json"), ProfileSelectedNodes);
            MoveIfNeeded(Path.Combine(ConfigDirectory, "profile_expanded_folders.json"), ProfileExpandedFolders);

            MoveIfNeeded(Path.Combine(ConfigDirectory, "template_organization.json"), TemplateOrganization);
            MoveIfNeeded(Path.Combine(ConfigDirectory, "template_locked_nodes.json"), TemplateLockedNodes);
            MoveIfNeeded(Path.Combine(ConfigDirectory, "template_selected_nodes.json"), TemplateSelectedNodes);
            MoveIfNeeded(Path.Combine(ConfigDirectory, "template_expanded_folders.json"), TemplateExpandedFolders);
        }
        catch
        {
            // Non-fatal: if anything goes wrong with migration, the plugin will just create
            // fresh organization files in the new location. Actual profile/template data is
            // never touched by this method.
        }
    }

    private static void MoveIfNeeded(string oldPath, string newPath)
    {
        if (!File.Exists(oldPath))
            return;

        if (File.Exists(newPath))
        {
            // New location already has a file - assume it's authoritative and remove the stale old file.
            try { File.Delete(oldPath); } catch { /* ignore */ }
            return;
        }

        File.Move(oldPath, newPath);
    }

    public override List<FileInfo> GetBackupFiles()
        => [];

    public IEnumerable<FileInfo> Templates()
    {
        if (!Directory.Exists(TemplateDirectory))
            yield break;

        foreach (var file in Directory.EnumerateFiles(TemplateDirectory, "*.json", SearchOption.TopDirectoryOnly))
            yield return new FileInfo(file);
    }

    public string TemplateFile(Guid id)
        => Path.Combine(TemplateDirectory, $"{id}.json");

    public string TemplateFile(Templates.Data.Template template)
        => TemplateFile(template.UniqueId);

    public IEnumerable<FileInfo> Profiles()
    {
        if (!Directory.Exists(ProfileDirectory))
            yield break;

        foreach (var file in Directory.EnumerateFiles(ProfileDirectory, "*.json", SearchOption.TopDirectoryOnly))
            yield return new FileInfo(file);
    }

    public string ProfileFile(Guid id)
        => Path.Combine(ProfileDirectory, $"{id}.json");

    public string ProfileFile(Profile profile)
        => ProfileFile(profile.UniqueId);
}
