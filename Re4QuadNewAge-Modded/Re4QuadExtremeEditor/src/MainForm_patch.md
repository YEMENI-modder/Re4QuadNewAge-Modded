# MainForm.cs — Patch Instructions

## Change — Call StartPreloadFormCaches in GlControl_Load

Open `MainForm.cs` and find `private void GlControl_Load(...)`.
Inside the `if (theAppLoadedWell)` block, find this section near the end:

**FIND:**
```csharp
                DataShader.StartLoad();
                Utils.StartLoadObjsModels();

                glControl.SwapBuffers();

                SplashScreen.Conteiner?.Close?.Invoke();
```

**REPLACE WITH:**
```csharp
                DataShader.StartLoad();
                Utils.StartLoadObjsModels();

                // Preload SelectRoomForm + OptionsForm data while splash is still visible
                Utils.StartPreloadFormCaches();

                glControl.SwapBuffers();

                SplashScreen.Conteiner?.Close?.Invoke();
```

---

That single call is everything needed in MainForm.  
The preload happens behind the splash screen so the user sees no extra startup delay.
