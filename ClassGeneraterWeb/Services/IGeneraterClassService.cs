using ClassGeneraterWeb.Controllers;
using ClassGeneraterWeb.Models;

namespace ClassGeneraterWeb.Services
{
    public interface IGeneraterClassService
    {
        public (string pocoClass, string ErrorMessage) GeneraterClass(GeneraterClassAction generaterClassAction);
    }
}