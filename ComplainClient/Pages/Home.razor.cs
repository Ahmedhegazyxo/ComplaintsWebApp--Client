using System.Net.Http.Json;
using Base;
namespace ComplainClient.Pages;

public partial class Home
{
    private void NavigateToInquireAboutComplain()
    {
        _nvmgr.NavigateTo("/InquireAboutComplain");
    }
    private void NavigateToAddComplains()
    {
        _nvmgr.NavigateTo("/AddComplain");
    }
  
}