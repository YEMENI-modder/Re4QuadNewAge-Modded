# OptionsForm.cs — Patch Instructions

Open `OptionsForm.cs` and find the constructor `OptionsForm()`.

## Change 1 — Use cache for the 4 JSON list combos

**FIND** (lines ~40–48):
```csharp
            //aba 2
            var EnemiesLists = GetEnemiesListJson();
            var EtcModelsLists = GetEtcModelsListJson();
            var ItemsLists = GetItemsListJson();
            var QuadCustomLists = GetQuadCustomListJson();
```

**REPLACE WITH:**
```csharp
            //aba 2  —  use preloaded cache (instant); fallback to disk only if not ready
            var EnemiesLists   = DataBase.CachedEnemiesLists    ?? GetEnemiesListJson();
            var EtcModelsLists = DataBase.CachedEtcModelsLists  ?? GetEtcModelsListJson();
            var ItemsLists     = DataBase.CachedItemsLists      ?? GetItemsListJson();
            var QuadCustomLists = DataBase.CachedQuadCustomLists ?? GetQuadCustomListJson();
```

---

## Change 2 — Use cache for the language list combo

**FIND** (lines ~99–100):
```csharp
            comboBoxLanguage.Items.Add(Lang.GetText(eLang.OptionsUseInternalLanguage));
            comboBoxLanguage.Items.AddRange(GetLangList());
```

**REPLACE WITH:**
```csharp
            comboBoxLanguage.Items.Add(Lang.GetText(eLang.OptionsUseInternalLanguage));
            var langList = DataBase.CachedLangList ?? GetLangList();
            comboBoxLanguage.Items.AddRange(langList);
```

---

That's it for OptionsForm.  
The private `GetEnemiesListJson()`, `GetEtcModelsListJson()`, `GetItemsListJson()`,
`GetQuadCustomListJson()`, and `GetLangList()` methods stay exactly as they are —
they are only called now if the cache is null (i.e., the very first startup before
`StartPreloadFormCaches` completes, which should never happen in practice).
