namespace LibHIRT.TagReader.Readers
{
    public class BaseTemplate : IBaseTemplate
    {
        bool _loading = false;
        bool _loaded = false;

        public void AddSubForOnInstanceLoad(object objMethod)
        {
            throw new NotImplementedException();
        }

        public string GetTagGroup()
        {
            throw new NotImplementedException();
        }

        public bool IsLoaded()
        {
            throw new NotImplementedException();
        }

        public void Load(bool force = false)
        {
            throw new NotImplementedException();
        }

        public void onInstanceLoad(object instance)
        {
            throw new NotImplementedException();
        }

        public void ReadParameterByName()
        {
            throw new NotImplementedException();
        }

        public void RemoveSubForOnInstanceLoad(object objMethod)
        {
            throw new NotImplementedException();
        }

        public string ToJson()
        {
            throw new NotImplementedException();
        }
    }
}
