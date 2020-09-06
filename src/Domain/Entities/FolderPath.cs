using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace PlexRipper.Domain
{
    public class FolderPath : BaseEntity
    {
        /// <summary>
        /// The folder type, do now edit or access this property directly. Use the FolderType property!
        /// </summary>
        [Column(Order = 1)]
        public string Type { get; set; }

        [Column(Order = 2)]
        public string DisplayName { get; set; }

        [Column(Order = 3)]
        public string DirectoryPath { get; set; }


        #region Helpers

        [NotMapped]
        public FolderType FolderType
        {
            get => Enum.TryParse(Type, out FolderType status) ? status : FolderType.Unknown;
            set => Type = value.ToString();
        }

        public bool IsValid()
        {
            return Directory.Exists(DirectoryPath);
        }

        #endregion
    }
}