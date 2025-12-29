using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe Data", menuName = "ScriptableObjects/RecipeData")]
public class RecipeData : ScriptableObject
{
    public string recipeName;
    public int score;
    //public IngredientCountDictionary requiredIngredientsPices;
    [field: SerializeField] public List<RecipeStep> RequiredIngredients { get; private set; }
}

//[Serializable]
//public class IngredientCountDictionary
//{
//    [SerializeField] private List<IngredientData> keys = new();
//    [SerializeField] private List<int> values = new();

//    private Dictionary<IngredientData, int> _dictionary;

//    public Dictionary<IngredientData, int> ToDictionary()
//    {
//        if (_dictionary != null) return _dictionary;

//        _dictionary = new Dictionary<IngredientData, int>();
//        for (int i = 0; i < Mathf.Min(keys.Count, values.Count); i++)
//        {
//            if (!keys[i]) continue; // skip nulls
//            if (!_dictionary.ContainsKey(keys[i]))
//                _dictionary.Add(keys[i], values[i]);
//        }

//        return _dictionary;
//    }

//    public void FromDictionary(Dictionary<IngredientData, int> dict)
//    {
//        keys.Clear();
//        values.Clear();

//        foreach (var kvp in dict)
//        {
//            keys.Add(kvp.Key);
//            values.Add(kvp.Value);
//        }

//        _dictionary = new Dictionary<IngredientData, int>(dict);
//    }

//    public void Add(IngredientData key, int amount)
//    {
//        if (key == null) return;

//        // Check if already exists in serialized list
//        int index = keys.IndexOf(key);
//        if (index >= 0)
//        {
//            values[index] = amount; // overwrite
//        }
//        else
//        {
//            keys.Add(key);
//            values.Add(amount);
//        }

//        // Update cached dictionary
//        _dictionary ??= new Dictionary<IngredientData, int>();
//        _dictionary[key] = amount;
//    }

//    public List<KeyValuePair<IngredientData, int>> ToList()
//    {
//        var list = new List<KeyValuePair<IngredientData, int>>();
//        for (int i = 0; i < Mathf.Min(keys.Count, values.Count); i++)
//        {
//            if (keys[i] != null)
//                list.Add(new KeyValuePair<IngredientData, int>(keys[i], values[i]));
//        }
//        return list;
//    }

//    public void Clear()
//    {
//        _dictionary = null;
//    }
//}
