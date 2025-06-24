using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Product_API.Common.Constants;
using Product_API.Domain.Entities;
using Product_API.Domain.Interfaces;
using Product_API.Common.DTO;

namespace Product_API.Application.Services
{
    public class ProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly CategoryHttpClientService _categoryHttpClientService;

        public ProductService(IProductRepository productRepository, CategoryHttpClientService categoryHttpClientService)
        {
            _productRepository = productRepository;
            _categoryHttpClientService = categoryHttpClientService;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _productRepository.GetAllAsync();
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            return await _productRepository.GetByIdAsync(id);
        }

        public async Task<Product> AddAsync(Product product)
        {
            // Validate product
            if (string.IsNullOrWhiteSpace(product.Name))
                throw new ArgumentException(AppConstants.Validation.RequiredName);

            if (product.Price <= 0)
                throw new ArgumentException(AppConstants.Validation.InvalidPrice);

            // Validate CategoryID
            var categories = await _categoryHttpClientService.GetCategoriesAsync();
            if (categories == null || !categories.Any(c => c.Id == product.CategoryID))
                throw new ArgumentException("CategoryID is invalid");

            await _productRepository.AddAsync(product);
            return product;
        }

        public async Task<Product> UpdateAsync(Guid id, Product product)
        {
            // Validate product
            if (string.IsNullOrWhiteSpace(product.Name))
                throw new ArgumentException(AppConstants.Validation.RequiredName);

            if (product.Price <= 0)
                throw new ArgumentException(AppConstants.Validation.InvalidPrice);

            // Validate CategoryID
            var categories = await _categoryHttpClientService.GetCategoriesAsync();
            if (categories == null || !categories.Any(c => c.Id == product.CategoryID))
                throw new ArgumentException("CategoryID is invalid");

            var existingProduct = await _productRepository.GetByIdAsync(id);
            if (existingProduct == null)
                throw new KeyNotFoundException(string.Format(AppConstants.Validation.ProductNotFound, id));

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.CategoryID = product.CategoryID;

            await _productRepository.UpdateAsync(existingProduct);
            return existingProduct;
        }

        public async Task RemoveAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                throw new KeyNotFoundException(string.Format(AppConstants.Validation.ProductNotFound, id));

            await _productRepository.DeleteAsync(id);
        }
    }
}
