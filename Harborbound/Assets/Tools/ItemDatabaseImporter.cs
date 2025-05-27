#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ItemDatabaseImporter : EditorWindow
{
    private ItemDatabase targetDatabase;
    private Object folderObject;
    
    [MenuItem("Tools/Item Database Importer")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(ItemDatabaseImporter), false, "Item Importer");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Import Items to Database", EditorStyles.boldLabel);
        
        targetDatabase = (ItemDatabase)EditorGUILayout.ObjectField(
            "Target Database:", targetDatabase, typeof(ItemDatabase), false);
        
        folderObject = EditorGUILayout.ObjectField(
            "Items Folder:", folderObject, typeof(Object), false);
        
        if (GUILayout.Button("Import All Items from Folder"))
        {
            if (targetDatabase == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a target database", "OK");
                return;
            }
            
            if (folderObject == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a folder", "OK");
                return;
            }
            
            string folderPath = AssetDatabase.GetAssetPath(folderObject);
            if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath))
            {
                EditorUtility.DisplayDialog("Error", "Selected object is not a valid folder", "OK");
                return;
            }
            
            ImportItemsFromFolder(folderPath);
        }
    }
    
    private void ImportItemsFromFolder(string folderPath)
    {
        // Find all ItemDefinitions in the folder and subfolders
        string[] guids = AssetDatabase.FindAssets("t:ItemDefinition", new string[] { folderPath });
        List<ItemDefinition> items = new List<ItemDefinition>();
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemDefinition item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);
            if (item != null)
                items.Add(item);
        }
        
        // Add the items to the database
        targetDatabase.allItems = items;
        
        // Save changes
        EditorUtility.SetDirty(targetDatabase);
        AssetDatabase.SaveAssets();
        
        EditorUtility.DisplayDialog("Import Successful", 
            $"Added {items.Count} items to the database", "OK");
    }
}
#endif
