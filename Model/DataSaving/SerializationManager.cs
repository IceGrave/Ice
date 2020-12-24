﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Whydoisuck.Model.DataStructures;
using Whydoisuck.Properties;

namespace Whydoisuck.DataSaving
{
    /// <summary>
    /// Manages serialization of differents objects in a given directory
    /// </summary>
    public static class SerializationManager
    {
        private static string _saveDirectory = null;
        /// <summary>
        /// Directory where all of the data is saved.
        /// </summary>
        public static string SaveDirectory {
            get
            {
                if(_saveDirectory == null)
                {
                    _saveDirectory = "./records/";//TODO
                }
                return _saveDirectory;
            }
            set
            {
                _saveDirectory = value;
            }
        }
        /// <summary>
        /// Path for the file containing the session manager.
        /// </summary>
        public static string IndexFilePath { get { return Path.Combine(SaveDirectory, IndexFileName); } }

        // name of the file containing the session manager
        private static string IndexFileName => Settings.Default.IndexerFileName;

        /// <summary>
        /// Initializes folders and files on the disk so that the session manager
        /// can work properly.
        /// </summary>
        public static void Init()
        {
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
            }
        }

        /// <summary>
        /// Saves a session on the disk
        /// </summary>
        /// <param name="group">The group the session belongs to</param>
        /// <param name="session">The session to save</param>
        public static void SerializeSession(SessionGroup group, Session session)
        {
            var path = Path.Combine(GetGroupDirectoryPath(group), GetSessionFileName(session));
            Serialize(path, session);
        }

        /// <summary>
        /// Moves current data to another folder
        /// </summary>
        /// <param name="path"></param>
        public static void MigrateData(string newPath)
        {
            //TODO
        }

        /// <summary>
        /// Creates a directory for a given group
        /// </summary>
        /// <param name="group">The group to create a directory for</param>
        public static bool CreateGroupDirectory(SessionGroup group)
        {
            var path = GetGroupDirectoryPath(group);
            try
            {
                Directory.CreateDirectory(path);
                return true;
            } catch (IOException)
            {
                return false;
            }
        }

        /// <summary>
        /// Deletes the directory associated to a group and all of its content
        /// </summary>
        /// <param name="group"></param>
        public static void DeleteGroupDirectory(SessionGroup group)
        {
            var path = GetGroupDirectoryPath(group);
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        /// <summary>
        /// Loads the sessions of a group
        /// </summary>
        /// <param name="group">The group to load the sessions of</param>
        /// <returns>The sessions of the group</returns>
        public static List<Session> LoadGroupSessions(SessionGroup group)
        {
            var res = new List<Session>();
            var folderPath = GetGroupDirectoryPath(group);
            var files = Directory.GetFiles(folderPath);//TODO maybe keep a list of session files ?
            foreach (var file in files)
            {
                var session = (Session)Deserialize(file, new Session());
                res.Add(session);
            }
            return res;
        }

        /// <summary>
        /// Saves the session manager on the disk
        /// </summary>
        /// <param name="manager"></param>
        public static void SerializeSessionManager(SessionManager manager)
        {
            Serialize(IndexFilePath, manager);
        }

        /// <summary>
        /// Loads the manager from the file on the disk
        /// </summary>
        /// <returns>The loaded session manager</returns>
        public static void DeserializeSessionManager(SessionManager manager)
        {
            if (File.Exists(IndexFilePath))
            {
                Deserialize(IndexFilePath, manager);
            }
        }

        /// <summary>
        /// Saves a serializable object to a given file path
        /// </summary>
        /// <param name="filePath">Where the object will be saved</param>
        /// <param name="item">The object to serialize</param>
        public static void Serialize(string filePath, IWDISSerializable item)
        {
            var serializedItem = item.Serialize();
            File.WriteAllText(filePath, serializedItem);
        }

        /// <summary>
        /// Loads an object from it's file on the disk
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>The loaded object</returns>
        public static IWDISSerializable Deserialize(string filePath, IWDISSerializable item)
        {
            var value = File.ReadAllText(filePath);
            //Updating the object if needed
            var jo = JObject.Parse(value);
            if(!item.CurrentVersionCompatible((int)jo[IWDISSerializable.VersionPropertyName]))
            {
                item.UpdateOldVersion(ref jo);
                File.WriteAllText(filePath, jo.ToString());
            }
            item.Deserialize(jo.ToString());
            return item;
        }

        // Gets the path of the directory of a group.
        private static string GetGroupDirectoryPath(SessionGroup group)
        {
            var path = Path.Combine(SaveDirectory, group.GroupName.Trim());
            return path;
        }

        // Gets to file name for a session
        private static string GetSessionFileName(Session session)
        {
            return session.SessionName;
        }
    }
}
