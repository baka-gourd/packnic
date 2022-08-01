namespace Packnic.Core;

public class Resources
{
    private static GlobalDbContext? _globalDb;

    public static GlobalDbContext GlobalDb
    {
        get
        {
            if (_globalDb is null)
            {
                _globalDb = new GlobalDbContext();
                _globalDb.Database.EnsureCreated();
            }

            return _globalDb;
        }
    }
}