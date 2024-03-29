﻿using HaloInfiniteResearchTools.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HaloInfiniteResearchTools.Services
{
    public class PreferencesService : IPreferencesService
    {

        #region Data Members

        private static readonly AutoResetEvent _ioLock = new AutoResetEvent(true);

        #endregion

        #region Properties

        public PreferencesModel Preferences { get; private set; }

        #endregion

        #region Public Methods

        public async Task Initialize()
        {
            if (Preferences != null)
                return;

            await LoadPreferences();

            if (Preferences is null)
            {
                Preferences = PreferencesModel.Default;
                await SavePreferences();
            }
            else
                EnsurePreferencesAreSet();
        }

        public async Task<PreferencesModel> LoadPreferences()
        {
            try
            {
                _ioLock.WaitOne();

                using var fs = File.OpenRead(GetPreferencesPath());
                Preferences = await JsonSerializer.DeserializeAsync<PreferencesModel>(fs);
            }
            catch (Exception ex)
            {
            }
            finally
            {
                _ioLock.Set();
            }

            return Preferences;
        }

        public async Task SavePreferences()
        {
            try
            {
                _ioLock.WaitOne();

                using var fs = File.Create(GetPreferencesPath());
                await JsonSerializer.SerializeAsync(fs, Preferences);
            }
            catch (Exception ex)
            {
            }
            finally
            {
                _ioLock.Set();
            }
        }

        #endregion

        #region Private Methods

        private string GetPreferencesPath()
        {
            var userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(userPath, "H2AIndex.prefs");
        }

        private void EnsurePreferencesAreSet()
        {
            if (Preferences.ModelExportOptions is null)
                Preferences.ModelExportOptions = ModelExportOptionsModel.Default;
            if (Preferences.ModelViewerOptions is null)
                Preferences.ModelViewerOptions = ModelViewerOptionsModel.Default;
            if (Preferences.TextureExportOptions is null)
                Preferences.TextureExportOptions = TextureExportOptionsModel.Default;
            if (Preferences.TextureViewerOptions is null)
                Preferences.TextureViewerOptions = TextureViewerOptionsModel.Default;
            if (Preferences.TagStructsDumperOptions is null)
                Preferences.TagStructsDumperOptions = TagStructsDumperOptionsModel.Default;

        }

        #endregion

    }
}
