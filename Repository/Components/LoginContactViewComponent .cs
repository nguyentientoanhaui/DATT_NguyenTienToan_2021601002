using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Shopping_Demo.Repository.Components
{
    public class LoginContactViewComponent : ViewComponent
    {
        private readonly DataContext _dataContext;

        public LoginContactViewComponent(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var contact = await _dataContext.Contact.FirstOrDefaultAsync();
            return View(contact);
        }
    }
}
