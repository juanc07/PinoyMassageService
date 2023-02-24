using PinoyMassageService.Dtos;
using static PinoyMassageService.Dtos.ContactDtos;

namespace PinoyMassageService.ResponseObject
{
    public class ContactsData
    {
        public List<ContactDto> Contacts { get; set; }
    }
}
