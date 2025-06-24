namespace Catalog_API.Common.Constants
{
    public static class AppConstants
    {
        public static class Validation
        {
            public const string RequiredName = "Tên danh mục là bắt buộc.";
            public const string NameTooLong = "Tên danh mục không được vượt quá 100 ký tự.";
            public const string DescriptionTooLong = "Mô tả không được vượt quá 255 ký tự.";
            public const string CategoryNotFound = "Không tìm thấy danh mục với id: {0}";
        }
    }
} 