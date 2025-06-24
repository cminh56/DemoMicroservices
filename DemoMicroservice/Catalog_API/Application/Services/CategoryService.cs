using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Catalog_API.Common.Constants;
using Catalog_API.Domain.Entities;
using Catalog_API.Domain.Interfaces;

namespace Catalog_API.Application.Services
{
    public class CategoryService
    {
        private readonly ICategoryRepository _repository;
        public CategoryService(ICategoryRepository repository)
        {
            _repository = repository;
        }
        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }
        public async Task<Category?> GetByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }
        public async Task<Category> AddAsync(Category category)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(category.Name))
                throw new ArgumentException(AppConstants.Validation.RequiredName);
            if (category.Name.Length > 100)
                throw new ArgumentException(AppConstants.Validation.NameTooLong);
            if (category.Description != null && category.Description.Length > 255)
                throw new ArgumentException(AppConstants.Validation.DescriptionTooLong);
            return await _repository.AddAsync(category);
        }
        public async Task<Category> UpdateAsync(Guid id, Category category)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
                throw new ArgumentException(AppConstants.Validation.RequiredName);
            if (category.Name.Length > 100)
                throw new ArgumentException(AppConstants.Validation.NameTooLong);
            if (category.Description != null && category.Description.Length > 255)
                throw new ArgumentException(AppConstants.Validation.DescriptionTooLong);
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException(string.Format(AppConstants.Validation.CategoryNotFound, id));
            existing.Name = category.Name;
            existing.Description = category.Description;
            await _repository.UpdateAsync(existing);
            return existing;
        }
        public async Task RemoveAsync(Guid id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException(string.Format(AppConstants.Validation.CategoryNotFound, id));
            await _repository.DeleteAsync(id);
        }
    }
} 