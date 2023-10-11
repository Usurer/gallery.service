﻿namespace Api.Services
{
    public interface IFileOrFolderInfo
    {
        public long Id
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        public bool IsFolder
        {
            get;
        }
    }
}