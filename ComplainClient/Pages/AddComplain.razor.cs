using Base;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;
using System.Reflection;

namespace ComplainClient.Pages;

public partial class AddComplain
{
    private bool isLoading = false;
    Complain complain = new Complain();

    List<ComplainStatus> complainStatus = new List<ComplainStatus>();
    List<ComplainType> complainTypes = new List<ComplainType>();
    List<MilitaryRank> militaryRanks = new List<MilitaryRank>();

    private string subjectInComplainStatement = string.Empty;
    private string complainMessage = string.Empty;
    private string complainBodyTemplate = $"{ProjectResources.ToDepartementPresidentMesssage}" + " "
                                        + $"{ProjectResources.AppliedToYouMessage}" + " "
                                        + $"{ProjectResources.MilitaryNumber}" + "/" + "{0}" + " "
                                        + $"{ProjectResources.Rank}" + "/" + "{1}" + " "
                                        + $"{ProjectResources.Name}" + "/" + "{2}" + " "
                                        + $"{ProjectResources.FromNativeUnitOf}" + "/" + "{3}" + " "
                                        + $"{ProjectResources.FromCurrentUnitOf}" + "/" + "{4}" + " "
                                        + $"{ProjectResources.SubjectOfComplainTemplate}" + "  " + "{5}" + " "
                                        + $"{ProjectResources.ComplainRequest}" + "  " + "{6}" + " ";

    private string ssnErrorMessage;
    private string nameErrorMessage;
    private string militaryNumberErrorMessage;
    private string mainPhoneNumberErrorMessage;
    private string rankErrorMessage;
    private string complainBodyErrorMessage;
    private string nativeUnitErrorMessage;
    private string currentUnitErrorMessage;
    private string militaryCardAttachmentErrorMessage;
    private string nationalCardAttachmentErrorMessage;
    private string aboutComplainAttachmentErrorMessage;

    ComplainAttachment militaryCardAttachment = new ComplainAttachment
    {
        Attachment = new Attachment
        {
            AttachmentAttribute = "صورة الكارنيه"
        }
    };
    ComplainAttachment nationalCardAttachment = new ComplainAttachment
    {
        Attachment = new Attachment
        {
            AttachmentAttribute = "صورة البطاقة"
        }
    };
    ComplainAttachment aboutComplainAttachment = new ComplainAttachment
    {
        Attachment = new Attachment
        {
            AttachmentAttribute = "مرفق خاص بالشكوى"
        }
    };
    private bool isShowModal = false;
    private Complain savedComplain = new Complain();
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {

            complainStatus = await _client.GetFromJsonAsync<List<ComplainStatus>>("api/complainStatus");
            complainTypes = await _client.GetFromJsonAsync<List<ComplainType>>("api/complainType");
            militaryRanks = await _client.GetFromJsonAsync<List<MilitaryRank>>("api/militaryRank");
            if (complainStatus?.Any() == true)
                complain.ComplainStatusId = complainStatus.FirstOrDefault().Id;
            if (complainTypes?.Any() == true)
            {
                complain.ComplainTypeId = complainTypes.FirstOrDefault().Id;
            }
            if (militaryRanks?.Any() == true)
            {
                complain.MilitaryRankId = militaryRanks.FirstOrDefault().Id;
            }
            await InvokeAsync(ComplainBodyConcatination);
            }
            catch(Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
        }
        await base.OnAfterRenderAsync(firstRender);
    }
    protected override async Task OnInitializedAsync()
    {
        complain.CreationDate = DateTime.Now;
        await base.OnInitializedAsync();
    }
    private async Task OnInputFileChange(InputFileChangeEventArgs e, AttachmentType attachmentType)
    {
        nationalCardAttachmentErrorMessage = string.Empty;
        militaryCardAttachmentErrorMessage = string.Empty;
        aboutComplainAttachmentErrorMessage = string.Empty;

        var file = e.File;
        const long maxAllowedSize = 5 * 1024 * 1024;

        if (!(file.Size > maxAllowedSize))
        {


            var buffer = new byte[file.Size];

            complain.ComplainStatusId = await file.OpenReadStream(maxAllowedSize).ReadAsync(buffer);

            if (attachmentType == AttachmentType.MilitaryCard)
            {
                militaryCardAttachment.Attachment.Content = buffer;
                militaryCardAttachment.Attachment.FileName = file.Name;
                militaryCardAttachment.Attachment.FileType = "." + file.Name.Split('.').Last();
            }
            else if (attachmentType == AttachmentType.NationalId)
            {
                nationalCardAttachment.Attachment.Content = buffer;
                nationalCardAttachment.Attachment.FileName = file.Name;
                nationalCardAttachment.Attachment.FileType = "." + file.Name.Split('.').Last();
            }
            else if (attachmentType == AttachmentType.ComplainAttachment)
            {
                aboutComplainAttachment.Attachment.Content = buffer;
                aboutComplainAttachment.Attachment.FileName = file.Name;
                aboutComplainAttachment.Attachment.FileType = "." + file.Name.Split('.').Last();
            }
        }
        else
        {
            if (attachmentType == AttachmentType.MilitaryCard)
                militaryCardAttachmentErrorMessage = ProjectResources.FileMaxSize;
            else if (attachmentType == AttachmentType.NationalId)
                nationalCardAttachmentErrorMessage = ProjectResources.FileMaxSize;
            else if (attachmentType == AttachmentType.ComplainAttachment)
                aboutComplainAttachmentErrorMessage = ProjectResources.FileMaxSize;
        }
    }
    private void ValidateBeforeSubmit(Complain complain)
    {
        ssnErrorMessage = string.Empty;
        nameErrorMessage = string.Empty;
        militaryNumberErrorMessage = string.Empty;
        mainPhoneNumberErrorMessage = string.Empty;
        rankErrorMessage = string.Empty;
        complainBodyErrorMessage = string.Empty;
        nativeUnitErrorMessage = string.Empty;
        currentUnitErrorMessage = string.Empty;
        militaryCardAttachmentErrorMessage = string.Empty;
        nationalCardAttachmentErrorMessage = string.Empty;
        aboutComplainAttachmentErrorMessage = string.Empty;

        complain.IsValidEntity = true;

        if (string.IsNullOrEmpty(complain.NationalId))
        {
            ssnErrorMessage = ProjectResources.IsRequired;
            complain.IsValidEntity = false;
        }
        else if (complain.NationalId.Length != 14 || !complain.NationalId.All(char.IsDigit))
        {
            ssnErrorMessage = ProjectResources.SSNValidationMessage;
            complain.IsValidEntity = false;
        }

        if (string.IsNullOrEmpty(complain.Name))
        {
            nameErrorMessage = ProjectResources.IsRequired;
            complain.IsValidEntity = false;
        }
        else if (complain.Name.Length < 10 || complain.Name.Length > 50)
        {
            nameErrorMessage = ProjectResources.NameMaxLength;
            complain.IsValidEntity = false;
        }
        if (string.IsNullOrEmpty(complain.MilitaryNumber))
        {
            militaryNumberErrorMessage = ProjectResources.IsRequired;
            complain.IsValidEntity = false;
        }

        if (string.IsNullOrEmpty(complain.PhoneNumber))
        {
            mainPhoneNumberErrorMessage = ProjectResources.IsRequired;
            complain.IsValidEntity = false;
        }
        if (string.IsNullOrEmpty(complain.ComplainBody))
        {
            complainBodyErrorMessage = ProjectResources.IsRequired;
            complain.IsValidEntity = false;
        }
        if (string.IsNullOrEmpty(complain.NativeUnit))
        {
            nativeUnitErrorMessage = ProjectResources.IsRequired;
            complain.IsValidEntity = false;
        }
        if (string.IsNullOrEmpty(complain.CurrentUnit))
        {
            currentUnitErrorMessage = ProjectResources.IsRequired;
            complain.IsValidEntity = false;
        }
        if (string.IsNullOrEmpty(nationalCardAttachment.Attachment.Content?.ToString()))
        {
            nationalCardAttachmentErrorMessage = ProjectResources.IsRequired;
            complain.IsValidEntity = false;
        }
        if (string.IsNullOrEmpty(militaryCardAttachment.Attachment.Content?.ToString()))
        {
            militaryCardAttachmentErrorMessage = ProjectResources.IsRequired;
            complain.IsValidEntity = false;
        }
        StateHasChanged();
    }
    private void OnComplainTypeIdChanged(int? complainTypeId)
    {
        complain.ComplainTypeId = complainTypeId;
        ComplainBodyConcatination();
    }
    private void OnMilitaryNumberChanged(string militaryNumber)
    {
        complain.MilitaryNumber = militaryNumber;
        ComplainBodyConcatination();
    }
    private void OnMilitaryRankChanged(int militaryRankId)
    {
        complain.MilitaryRankId = militaryRankId;
        ComplainBodyConcatination();
    }
    private void ComplainMessageChanged(string message)
    {
        complainMessage = message;
        ComplainBodyConcatination();
    }

    private void OnNameChanged(string name)
    {
        complain.Name = name;
        ComplainBodyConcatination();
    }
    private void OnSubjectInComplainStatementChanged(string subject)
    {
        subjectInComplainStatement = subject;
        ComplainBodyConcatination();
    }
    private void ComplainNativeUnitChanged(string nativeUnit)
    {
        complain.NativeUnit = nativeUnit;
        ComplainBodyConcatination();
    }
    private void ComplainCurrentUnitChanged(string currentUnit)
    {
        complain.CurrentUnit = currentUnit;
        ComplainBodyConcatination();
    }
    private void ComplainBodyConcatination()
    {
        complain.ComplainBody = string.Empty;   
        MilitaryRank militaryRank = militaryRanks.FirstOrDefault(e => e.Id == complain.MilitaryRankId);
        complain.ComplainBody = string.Format(complainBodyTemplate, complain.MilitaryNumber, militaryRank.Name, complain.Name, complain.NativeUnit, complain.CurrentUnit, subjectInComplainStatement, complainMessage);
        StateHasChanged();
    }
    private async Task HandleValidSubmit()
    {
        try
        {
            ValidateBeforeSubmit(complain);
            if (complain.IsValidEntity)
            {
                isLoading = true;
                isShowModal = true;
                await InvokeAsync(StateHasChanged);
                complain.ComplainAttachments = new List<ComplainAttachment>();
                if (militaryCardAttachment.Attachment.Content is not null)
                    complain.ComplainAttachments.Add(militaryCardAttachment);
                if (nationalCardAttachment.Attachment.Content is not null)
                    complain.ComplainAttachments.Add(nationalCardAttachment);
                if (aboutComplainAttachment.Attachment.Content is not null)
                    complain.ComplainAttachments.Add(aboutComplainAttachment);

                if (complainStatus is null || complainStatus.Count() == 0)
                    complain.ComplainStatusId = null;
                else
                {
                    complain.ComplainStatusId = complainStatus.First().Id;
                }

                if (complainTypes is null || complainTypes.Count() == 0)
                {
                    complain.ComplainTypeId = null;
                }
                else
                {
                    if (complain.ComplainTypeId == null)
                        complain.ComplainTypeId = complainTypes.First().Id;
                }
                HttpResponseMessage responseMessage = await _client.PostAsJsonAsync("api/Complain", complain);
                if (responseMessage.IsSuccessStatusCode)
                {
                    isShowModal = true;
                    complain = await responseMessage.Content.ReadFromJsonAsync<Complain>();
                    complain.MilitaryRank = militaryRanks.FirstOrDefault(e => e.Id == complain.MilitaryRankId);
                    savedComplain = complain;
                }
            }

        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
        }
        finally
        {
            isLoading = false;
        }
    }
    private async Task OnModalConfirm()
    {
        isShowModal = false;
        _navmgr.NavigateTo("/");
    }
    private void NavigateHome()
    {
        _navmgr.NavigateTo("/");
    }
    private void CancelForm()
    {
        complain = new();
        ssnErrorMessage = string.Empty;
        nameErrorMessage = string.Empty;
        militaryNumberErrorMessage = string.Empty;
        mainPhoneNumberErrorMessage = string.Empty;
        rankErrorMessage = string.Empty;
        complainBodyErrorMessage = string.Empty;
        nativeUnitErrorMessage = string.Empty;
        currentUnitErrorMessage = string.Empty;
        militaryCardAttachmentErrorMessage = string.Empty;
        nationalCardAttachmentErrorMessage = string.Empty;
        subjectInComplainStatement = string.Empty;
        complainMessage = string.Empty;
        StateHasChanged();
    }
    private enum AttachmentType
    {
        MilitaryCard = 0,
        NationalId = 1,
        ComplainAttachment = 2
    }
}