namespace Psychometric_Test_Designer.Core
{
    public static class PatchHelper
    {
        public static void ApplyNotNull<TDto, TEntity>(TDto dto, TEntity entity)
        {
            var dtoProps = typeof(TDto).GetProperties();

            foreach (var dtoProp in dtoProps)
            {
                var value = dtoProp.GetValue(dto);

                if (value == null)
                    continue;

                var entityProp = typeof(TEntity).GetProperty(dtoProp.Name);

                if (entityProp != null && entityProp.CanWrite)
                {
                    entityProp.SetValue(entity, value);
                }
            }
        }
    }
}
