﻿// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Humanizer;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// Block used to register for a registration instance.
    /// </summary>
    [DisplayName( "Registration Entry" )]
    [Category( "Event" )]
    [Description( "Block used to register for a registration instance." )]

    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS, "Connection Status", "The connection status to use for new individuals (default: 'Web Prospect'.)", true, false, "368DD475-242C-49C4-A42C-7278BE690CC2", "", 0 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS, "Record Status", "The record status to use for new individuals (default: 'Pending'.)", true, false, Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING, "", 1 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE, "Source", "The Financial Source Type to use when creating transactions", false, false, Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_WEBSITE, "", 2 )]
    [TextField( "Batch Name Prefix", "The batch prefix name to use when creating a new batch", false, "Event Registration", "", 3 )]
    [BooleanField( "Display Progress Bar", "Display a progress bar for the registration.", true, "", 4 )]
    [BooleanField( "Enable Debug", "Display the merge fields that are available for lava ( Success Page ).", false, "", 5 )]
    [SystemEmailField( "Confirm Account Template", "Confirm Account Email Template", false, Rock.SystemGuid.SystemEmail.SECURITY_CONFIRM_ACCOUNT, "", 4 )]
    public partial class RegistrationEntry : RockBlock
    {
        #region Fields

        private bool _saveNavigationHistory = false;

        // Page (query string) parameter names
        private const string REGISTRATION_ID_PARAM_NAME = "RegistrationId";
        private const string SLUG_PARAM_NAME = "Slug";
        private const string START_AT_BEGINNING = "StartAtBeginning";
        private const string REGISTRATION_INSTANCE_ID_PARAM_NAME = "RegistrationInstanceId";
        private const string EVENT_OCCURRENCE_ID_PARAM_NAME = "EventOccurrenceId";
        private const string GROUP_ID_PARAM_NAME = "GroupId";
        private const string CAMPUS_ID_PARAM_NAME = "CampusId";

        // Viewstate keys
        private const string REGISTRATION_INSTANCE_STATE_KEY = "RegistrationInstanceState";
        private const string REGISTRATION_STATE_KEY = "RegistrationState";
        private const string GROUP_ID_KEY = "GroupId";
        private const string CAMPUS_ID_KEY = "CampusId";
        private const string CURRENT_PANEL_KEY = "CurrentPanel";
        private const string CURRENT_REGISTRANT_INDEX_KEY = "CurrentRegistrantIndex";
        private const string CURRENT_FORM_INDEX_KEY = "CurrentFormIndex";
        private const string MINIMUM_PAYMENT_KEY = "MinimumPayment";

        // protected variables
        public double PercentComplete = 0;

        #endregion

        #region Properties

        // The selected registration instance
        private RegistrationInstance RegistrationInstanceState { get; set; }

        // The selected group from linkage
        private int? GroupId { get; set; }

        // The selected campus from event item occurrence or query string
        private int? CampusId { get; set; }

        // Info about each current registration
        protected RegistrationInfo RegistrationState { get; set; }

        // The current panel to display ( HowMany
        private int CurrentPanel { get; set; }

        // The current registrant index
        private int CurrentRegistrantIndex { get; set; }

        // The current form index
        private int CurrentFormIndex { get; set; }

        // The minimum payment that is due 
        private decimal? minimumPayment { get; set; }

        // The registration template.
        private RegistrationTemplate RegistrationTemplate
        {
            get
            {
                return RegistrationInstanceState != null ? RegistrationInstanceState.RegistrationTemplate : null;
            }
        }

        /// <summary>
        /// Gets the registration term.
        /// </summary>
        /// <value>
        /// The registration term.
        /// </value>
        private string RegistrationTerm
        {
            get
            {
                if ( RegistrationTemplate != null && !string.IsNullOrWhiteSpace( RegistrationTemplate.RegistrationTerm ) )
                {
                    return RegistrationTemplate.RegistrationTerm;
                }
                return "Registration";
            }
        }

        /// <summary>
        /// Gets the registrant term.
        /// </summary>
        /// <value>
        /// The registrant term.
        /// </value>
        private string RegistrantTerm
        {
            get
            {
                if ( RegistrationTemplate != null && !string.IsNullOrWhiteSpace( RegistrationTemplate.RegistrantTerm ) )
                {
                    return RegistrationTemplate.RegistrantTerm;
                }
                return "Person";
            }
        }

        /// <summary>
        /// Gets the fee term.
        /// </summary>
        /// <value>
        /// The fee term.
        /// </value>
        private string FeeTerm
        {
            get
            {
                if ( RegistrationTemplate != null && !string.IsNullOrWhiteSpace( RegistrationTemplate.FeeTerm ) )
                {
                    return RegistrationTemplate.FeeTerm;
                }
                return "Additional Option";
            }
        }

        /// <summary>
        /// Gets the discount code term.
        /// </summary>
        /// <value>
        /// The discount code term.
        /// </value>
        private string DiscountCodeTerm
        {
            get
            {
                if ( RegistrationTemplate != null && !string.IsNullOrWhiteSpace( RegistrationTemplate.DiscountCodeTerm ) )
                {
                    return RegistrationTemplate.DiscountCodeTerm;
                }
                return "Discount Code";
            }
        }

        /// <summary>
        /// Gets the number of forms for the current registration template.
        /// </summary>
        private int FormCount
        {
            get
            {
                if ( RegistrationTemplate != null && RegistrationTemplate.Forms != null )
                {
                    return RegistrationTemplate.Forms.Count;
                }

                return 0;
            }
        }
        
        /// <summary>
        /// If the registration template allows multiple registrants per registration, returns the maximum allowed
        /// </summary>
        private int MaxRegistrants
        {
            get
            {
                // If this is an existing registration, max registrants is the number of registrants already 
                // on registration ( don't allow adding new registrants )
                if ( RegistrationState != null && RegistrationState.RegistrationId.HasValue )
                {
                    return RegistrationState.RegistrantCount;
                }

                // Otherwise if template allows multiple, set the max amount
                if ( RegistrationTemplate != null && RegistrationTemplate.AllowMultipleRegistrants )
                {
                    if ( RegistrationTemplate.MaxRegistrants <= 0 )
                    {
                        return int.MaxValue;
                    }
                    return RegistrationTemplate.MaxRegistrants;
                }

                // Default is a maximum of one
                return 1;
            }
        }

        /// <summary>
        /// Gets the minimum number of registrants allowed. Most of the time this is one, except for an existing
        /// registration that has existing registrants. The minimum in this case is the number of existing registrants
        /// </summary>
        private int MinRegistrants
        {
            get
            {
                // If this is an existing registration, min registrants is the number of registrants already 
                // on registration ( don't allow adding new registrants )
                if ( RegistrationState != null && RegistrationState.RegistrationId.HasValue )
                {
                    return RegistrationState.RegistrantCount;
                }
                
                // Default is a minimum of one
                return 1;
            }
        }

        /// <summary>
        /// Gets or sets the progress bar steps.
        /// </summary>
        /// <value>
        /// The progress bar steps.
        /// </value>
        protected int ProgressBarSteps
        {
            get { return ViewState["ProgressBarSteps"] as int? ?? 1; }
            set { ViewState["ProgressBarSteps"] = value; }
        }

        /// <summary>
        /// Gets or sets the payment transaction code. Used to help double-charging
        /// </summary>
        protected string TransactionCode
        {
            get { return ViewState["TransactionCode"] as string ?? string.Empty; }
            set { ViewState["TransactionCode"] = value; }
        }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState[REGISTRATION_INSTANCE_STATE_KEY] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                SetRegistrationState();
            }
            else
            {
                RegistrationInstanceState = JsonConvert.DeserializeObject<RegistrationInstance>( json );
            }

            json = ViewState[REGISTRATION_STATE_KEY] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                RegistrationState = new RegistrationInfo();
            }
            else
            {
                RegistrationState = JsonConvert.DeserializeObject<RegistrationInfo>( json );
            }

            GroupId = ViewState[GROUP_ID_KEY] as int?;
            CampusId = ViewState[CAMPUS_ID_KEY] as int?;
            CurrentPanel = ViewState[CURRENT_PANEL_KEY] as int? ?? 0;
            CurrentRegistrantIndex = ViewState[CURRENT_REGISTRANT_INDEX_KEY] as int? ?? 0;
            CurrentFormIndex = ViewState[CURRENT_FORM_INDEX_KEY] as int? ?? 0;
            minimumPayment = ViewState[MINIMUM_PAYMENT_KEY] as decimal?;

            CreateDynamicControls( false );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            RegisterClientScript();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            // Reset warning/error messages
            nbMain.Visible = false;
            nbPaymentValidation.Visible = false;

            // register navigation event to enable support for the back button
            var sm = ScriptManager.GetCurrent( Page );
            //sm.EnableSecureHistoryState = false;
            sm.Navigate += sm_Navigate;

            // Show or Hide the Credit card entry panel based on if a saved account exists and it's selected or not.
            divNewCard.Style[HtmlTextWriterStyle.Display] = ( rblSavedCC.Items.Count == 0 || rblSavedCC.Items[rblSavedCC.Items.Count - 1].Selected ) ? "block" : "none";

            // Show save account info based on if checkbox is checked
            divSaveAccount.Style[HtmlTextWriterStyle.Display] = cbSaveAccount.Checked ? "block" : "none";

            if ( !Page.IsPostBack )
            {
                // Get the a registration if it has not already been loaded ( breadcrumbs may have loaded it )
                if ( RegistrationState != null || SetRegistrationState() )
                {
                    if ( RegistrationTemplate != null )
                    {
                        bool instanceFull = false;
                        if ( !RegistrationState.RegistrationId.HasValue && RegistrationInstanceState.MaxAttendees > 0 )
                        {
                            int registrants = RegistrationInstanceState.Registrations.Sum( r => r.Registrants.Count() );
                            instanceFull = registrants >= RegistrationInstanceState.MaxAttendees;
                        }

                        if ( instanceFull )
                        {
                            ShowWarning(
                                string.Format( "{0} Full", RegistrationTerm ),
                                string.Format( "There are not any more {0} available for {1}.", RegistrationTerm.ToLower().Pluralize(), RegistrationInstanceState.Name ) );

                        }
                        else
                        {
                            // Check Login Requirement
                            if ( RegistrationTemplate.LoginRequired && CurrentUser == null )
                            {
                                var site = RockPage.Site;
                                if ( site.LoginPageId.HasValue )
                                {
                                    site.RedirectToLoginPage( true );
                                }
                                else
                                {
                                    System.Web.Security.FormsAuthentication.RedirectToLoginPage();
                                }
                            }
                            else
                            {
                                // show the panel for asking how many registrants ( it may be skipped )
                                ShowHowMany();
                            }
                        }
                    }
                    else
                    {
                        ShowWarning( "Sorry", string.Format( "The selected {0} could not be found or is no longer active.", RegistrationTerm.ToLower() ) );
                    }
                }
            }
            else
            {
                // Load values from controls into the state objects
                ParseDynamicControls();
            }
        }

        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            if ( RegistrationInstanceState == null )
            {
                SetRegistrationState();
            }

            if ( RegistrationInstanceState != null )
            {
                RockPage.Title = RegistrationInstanceState.Name;
                breadCrumbs.Add( new BreadCrumb( RegistrationInstanceState.Name, pageReference ) );
                return breadCrumbs;
            }

            breadCrumbs.Add( new BreadCrumb( this.PageCache.PageTitle, pageReference ) );
            return breadCrumbs;
        }
        
        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState[REGISTRATION_INSTANCE_STATE_KEY] = JsonConvert.SerializeObject( RegistrationInstanceState, Formatting.None, jsonSetting );
            ViewState[REGISTRATION_STATE_KEY] = JsonConvert.SerializeObject( RegistrationState, Formatting.None, jsonSetting );

            ViewState[GROUP_ID_KEY] = GroupId;
            ViewState[CAMPUS_ID_KEY] = CampusId;
            ViewState[CURRENT_PANEL_KEY] = CurrentPanel;
            ViewState[CURRENT_REGISTRANT_INDEX_KEY] = CurrentRegistrantIndex;
            ViewState[CURRENT_FORM_INDEX_KEY] = CurrentFormIndex;
            ViewState[MINIMUM_PAYMENT_KEY] = minimumPayment;

            return base.SaveViewState();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            if ( _saveNavigationHistory )
            {
                if ( CurrentPanel != 1 )
                {
                    this.AddHistory( "event", string.Format( "{0},0,0", CurrentPanel ) );
                }
                else
                {
                    this.AddHistory( "event", string.Format( "1,{0},{1}", CurrentRegistrantIndex, CurrentFormIndex ) );
                }

            }

            base.OnPreRender( e );
        }

        #endregion

        #region Events

        #region Navigation Events

        /// <summary>
        /// Handles the Navigate event of the sm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="HistoryEventArgs"/> instance containing the event data.</param>
        void sm_Navigate( object sender, HistoryEventArgs e )
        {
            var state = e.State["event"];

            if ( CurrentPanel > 0 && state != null )
            {
                string[] commands = state.Split( ',' );

                int panelId = 0;
                int registrantId = 0;
                int formId = 0;

                if ( commands.Count() == 3 )
                {
                    panelId = Int32.Parse( commands[0] );
                    registrantId = Int32.Parse( commands[1] );
                    formId = Int32.Parse( commands[2] );
                }

                switch ( panelId )
                {
                    case 1:
                        {
                            CurrentRegistrantIndex = registrantId;
                            CurrentFormIndex = formId;
                            ShowRegistrant();
                            break;
                        }
                    case 2:
                        {
                            ShowSummary();
                            break;
                        }
                    default:
                        {
                            ShowHowMany();
                            break;
                        }
                }
            }
            else
            {
                ShowHowMany();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbHowManyNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbHowManyNext_Click( object sender, EventArgs e )
        {
            _saveNavigationHistory = true;

            CurrentRegistrantIndex = 0;
            CurrentFormIndex = 0;

            // Create registrants based on the number selected
            SetRegistrantState( numHowMany.Value );

            // set the max number of steps in the progress bar
            this.ProgressBarSteps = numHowMany.Value * FormCount + 2;
            
            PercentComplete = ( (double)2 / (double)ProgressBarSteps ) * 100;

            ShowRegistrant();

            hfTriggerScroll.Value = "true";
        }

        /// <summary>
        /// Handles the Click event of the lbRegistrantPrev control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRegistrantPrev_Click( object sender, EventArgs e )
        {
            if ( CurrentPanel == 1 )
            {
                _saveNavigationHistory = true;

                CurrentFormIndex--;
                if ( CurrentFormIndex < 0 )
                {
                    CurrentRegistrantIndex--;
                    CurrentFormIndex = FormCount - 1;
                }

                if ( CurrentRegistrantIndex < 0 )
                {
                    ShowHowMany();
                }
                else
                {
                    ShowRegistrant();

                    if ( CurrentRegistrantIndex == 0 && PageParameter( START_AT_BEGINNING ).AsBoolean() )
                    {
                        lbRegistrantPrev.Visible = false;
                    }
                }
            }
            else
            {
                ShowHowMany();
            }

            PercentComplete = ( ( (double)2 + ( ( CurrentFormIndex + 1 ) * CurrentRegistrantIndex ) ) / (double)ProgressBarSteps ) * 100;
                 
            hfTriggerScroll.Value = "true";
        }

        /// <summary>
        /// Handles the Click event of the lbRegistrantNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRegistrantNext_Click( object sender, EventArgs e )
        {
            if ( CurrentPanel == 1 )
            {
                _saveNavigationHistory = true;

                CurrentFormIndex++;
                if ( CurrentFormIndex >= FormCount )
                {
                    CurrentRegistrantIndex++;
                    CurrentFormIndex = 0;
                }

                if ( CurrentRegistrantIndex >= RegistrationState.RegistrantCount )
                {
                    ShowSummary();
                }
                else
                {
                    ShowRegistrant();
                }
            }
            else
            {
                ShowHowMany();
            }

            lbRegistrantPrev.Visible = true;

            PercentComplete = ( ( (double)2 + ( (CurrentFormIndex + 1) * CurrentRegistrantIndex ) ) / (double)ProgressBarSteps ) * 100;

            hfTriggerScroll.Value = "true";
        }

        /// <summary>
        /// Handles the Click event of the lbSummaryPrev control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSummaryPrev_Click( object sender, EventArgs e )
        {
            if ( CurrentPanel == 2 )
            {
                _saveNavigationHistory = true;

                CurrentRegistrantIndex = RegistrationState != null ? RegistrationState.RegistrantCount - 1 : 0;
                CurrentFormIndex = FormCount - 1;

                ShowRegistrant();
            }
            else
            {
                ShowHowMany();
            }

            PercentComplete = ( ( (double)2 + ( ( CurrentFormIndex + 1 ) * CurrentRegistrantIndex ) ) / (double)ProgressBarSteps ) * 100;

            hfTriggerScroll.Value = "true";
        }

        /// <summary>
        /// Handles the Click event of the lbSummaryNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSummaryNext_Click( object sender, EventArgs e )
        {
            if ( CurrentPanel == 2 )
            {
                List<string> paymentErrors = ValidatePayment();
                if ( !paymentErrors.Any() )
                {
                    _saveNavigationHistory = true;

                    var registrationId = SaveChanges();
                    if ( registrationId.HasValue )
                    {
                        ShowSuccess( registrationId.Value );
                    }
                    else
                    {
                        ShowSummary();
                    }
                }
                else
                {
                    nbPaymentValidation.Text = string.Format( "Please Correct the Following<ul><li>{0}</li></ul>", paymentErrors.AsDelimited( "</li><li>" ) );
                    nbPaymentValidation.Visible = true;
                    
                    ShowSummary();
                }
            }
            else
            {
                ShowHowMany();
            }

            hfTriggerScroll.Value = "true";
        }

        /// <summary>
        /// Handles the Click event of the lbConfirm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbConfirm_Click( object sender, EventArgs e )
        {
            if ( CurrentPanel == 2 )
            {
                _saveNavigationHistory = true;

                TransactionCode = string.Empty;

                var registrationId = SaveChanges();
                if ( registrationId.HasValue )
                {
                    ShowSuccess( registrationId.Value );
                }
                else
                {
                    ShowSummary();
                }
            }
        }

        #endregion

        #region Summary Panel Events 

        /// <summary>
        /// Handles the Click event of the lbDiscountApply control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDiscountApply_Click( object sender, EventArgs e )
        {
            if ( RegistrationState != null )
            {
                RegistrationState.DiscountCode = tbDiscountCode.Text;
                CreateDynamicControls( true );
            }
        }

        #endregion

        #region Success Panel Events

        /// <summary>
        /// Handles the Click event of the lbSaveAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSaveAccount_Click( object sender, EventArgs e )
        {
            if ( string.IsNullOrWhiteSpace( TransactionCode ) )
            {
                nbSaveAccount.Text = "Sorry, the account information cannot be saved as there's not a valid transaction code to reference";
                nbSaveAccount.Visible = true;
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                if ( phCreateLogin.Visible )
                {
                    if ( string.IsNullOrWhiteSpace( txtUserName.Text ) || string.IsNullOrWhiteSpace( txtPassword.Text ) )
                    {
                        nbSaveAccount.Title = "Missing Informaton";
                        nbSaveAccount.Text = "A username and password are required when saving an account";
                        nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                        nbSaveAccount.Visible = true;
                        return;
                    }

                    if ( new UserLoginService( rockContext ).GetByUserName( txtUserName.Text ) != null )
                    {
                        nbSaveAccount.Title = "Invalid Username";
                        nbSaveAccount.Text = "The selected Username is already being used.  Please select a different Username";
                        nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                        nbSaveAccount.Visible = true;
                        return;
                    }

                    if ( !UserLoginService.IsPasswordValid( txtPassword.Text ) )
                    {
                        nbSaveAccount.Title = string.Empty;
                        nbSaveAccount.Text = UserLoginService.FriendlyPasswordRules();
                        nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                        nbSaveAccount.Visible = true;
                        return;
                    }   
                    
                    if ( txtPasswordConfirm.Text != txtPassword.Text )
                    {
                        nbSaveAccount.Title = "Invalid Password";
                        nbSaveAccount.Text = "The password and password confirmation do not match";
                        nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                        nbSaveAccount.Visible = true;
                        return;
                    }
                }

                if ( !string.IsNullOrWhiteSpace( txtSaveAccount.Text ) )
                {
                    GatewayComponent gateway = null;
                    if ( RegistrationTemplate != null && RegistrationTemplate.FinancialGateway != null )
                    {
                        gateway = RegistrationTemplate.FinancialGateway.GetGatewayComponent();
                    }

                    if ( gateway != null )
                    {
                        var ccCurrencyType = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD ) );
                        string errorMessage = string.Empty;

                        PersonAlias authorizedPersonAlias = null;
                        string referenceNumber = string.Empty;
                        int? currencyTypeValueId = ccCurrencyType.Id;

                        var transaction = new FinancialTransactionService( rockContext ).GetByTransactionCode( TransactionCode );
                        if ( transaction != null && transaction.AuthorizedPersonAlias != null )
                        {
                            authorizedPersonAlias = transaction.AuthorizedPersonAlias;
                            if ( transaction.FinancialGateway != null )
                            {
                                transaction.FinancialGateway.LoadAttributes( rockContext );
                            }
                            referenceNumber = gateway.GetReferenceNumber( transaction, out errorMessage );
                        }

                        if ( authorizedPersonAlias != null && authorizedPersonAlias.Person != null )
                        {
                            if ( phCreateLogin.Visible )
                            {
                                var user = UserLoginService.Create(
                                    rockContext,
                                    authorizedPersonAlias.Person,
                                    Rock.Model.AuthenticationServiceType.Internal,
                                    EntityTypeCache.Read( Rock.SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() ).Id,
                                    txtUserName.Text,
                                    txtPassword.Text,
                                    false );

                                var mergeObjects = GlobalAttributesCache.GetMergeFields( null );
                                mergeObjects.Add( "ConfirmAccountUrl", RootPath + "ConfirmAccount" );

                                var personDictionary = authorizedPersonAlias.Person.ToLiquid() as Dictionary<string, object>;
                                mergeObjects.Add( "Person", personDictionary );

                                mergeObjects.Add( "User", user );

                                var recipients = new List<Rock.Communication.RecipientData>();
                                recipients.Add( new Rock.Communication.RecipientData( authorizedPersonAlias.Person.Email, mergeObjects ) );

                                try
                                {
                                    Rock.Communication.Email.Send( GetAttributeValue( "ConfirmAccountTemplate" ).AsGuid(), recipients, ResolveRockUrl( "~/" ), ResolveRockUrl( "~~/" ), false );
                                }
                                catch (Exception ex)
                                {
                                    ExceptionLogService.LogException( ex, Context, this.RockPage.PageId, this.RockPage.Site.Id, CurrentPersonAlias );
                                }
                            }

                            var paymentInfo = GetCCPaymentInfo( gateway );

                            if ( errorMessage.Any() )
                            {
                                nbSaveAccount.Title = "Invalid Transaction";
                                nbSaveAccount.Text = "Sorry, the account information cannot be saved. " + errorMessage;
                                nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                                nbSaveAccount.Visible = true;
                            }
                            else
                            {
                                if ( authorizedPersonAlias != null )
                                {
                                    var savedAccount = new FinancialPersonSavedAccount();
                                    savedAccount.PersonAliasId = authorizedPersonAlias.Id;
                                    savedAccount.ReferenceNumber = referenceNumber;
                                    savedAccount.Name = txtSaveAccount.Text;
                                    savedAccount.TransactionCode = TransactionCode;
                                    savedAccount.FinancialGatewayId = RegistrationTemplate.FinancialGateway.Id;
                                    savedAccount.FinancialPaymentDetail = new FinancialPaymentDetail();
                                    savedAccount.FinancialPaymentDetail.SetFromPaymentInfo( paymentInfo, gateway, rockContext );

                                    var savedAccountService = new FinancialPersonSavedAccountService( rockContext );
                                    savedAccountService.Add( savedAccount );
                                    rockContext.SaveChanges();

                                    cbSaveAccount.Visible = false;
                                    txtSaveAccount.Visible = false;
                                    phCreateLogin.Visible = false;
                                    divSaveActions.Visible = false;

                                    nbSaveAccount.Title = "Success";
                                    nbSaveAccount.Text = "The account has been saved for future use";
                                    nbSaveAccount.NotificationBoxType = NotificationBoxType.Success;
                                    nbSaveAccount.Visible = true;
                                }
                            }
                        }
                        else
                        {
                            nbSaveAccount.Title = "Invalid Transaction";
                            nbSaveAccount.Text = "Sorry, the account information cannot be saved as there's not a valid transaction code to reference.";
                            nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                            nbSaveAccount.Visible = true;
                        }
                    }
                    else
                    {
                        nbSaveAccount.Title = "Invalid Gateway";
                        nbSaveAccount.Text = "Sorry, the financial gateway information for this type of transaction is not valid.";
                        nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                        nbSaveAccount.Visible = true;
                    }
                }
                else
                {
                    nbSaveAccount.Title = "Missing Account Name";
                    nbSaveAccount.Text = "Please enter a name to use for this account.";
                    nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                    nbSaveAccount.Visible = true;
                }
            }
        }
    


        #endregion

        #endregion

        #region Methods

        #region State Methods

        /// <summary>
        /// Sets the registration state
        /// </summary>
        private bool SetRegistrationState()
        {
            string registrationSlug = PageParameter( SLUG_PARAM_NAME );
            int? registrationInstanceId = PageParameter( REGISTRATION_INSTANCE_ID_PARAM_NAME ).AsIntegerOrNull();
            int? registrationId = PageParameter( REGISTRATION_ID_PARAM_NAME ).AsIntegerOrNull();
            int? groupId = PageParameter( GROUP_ID_PARAM_NAME ).AsIntegerOrNull();
            int? campusId = PageParameter( CAMPUS_ID_PARAM_NAME ).AsIntegerOrNull();
            int? eventOccurrenceId = PageParameter( EVENT_OCCURRENCE_ID_PARAM_NAME ).AsIntegerOrNull();

            // Not inside a "using" due to serialization needing context to still be active
            var rockContext = new RockContext();

            // An existing registration id was specified
            if ( registrationId.HasValue)
            {
                var registrationService = new RegistrationService( rockContext );
                var registration = registrationService
                    .Queryable( "Registrants.PersonAlias.Person,Registrants.GroupMember,RegistrationInstance.Account,RegistrationInstance.RegistrationTemplate.Fees,RegistrationInstance.RegistrationTemplate.Discounts,RegistrationInstance.RegistrationTemplate.Forms.Fields.Attribute,RegistrationInstance.RegistrationTemplate.FinancialGateway" )
                    .Where( r => r.Id == registrationId.Value )
                    .FirstOrDefault();

                if ( registration != null  && CurrentPersonId.HasValue )
                {
                    if ( ( registration.PersonAlias != null && registration.PersonAlias.PersonId == CurrentPersonId.Value ) ||
                        ( registration.CreatedByPersonAlias != null && registration.CreatedByPersonAlias.PersonId == CurrentPersonId.Value ) )
                    {
                        RegistrationInstanceState = registration.RegistrationInstance;
                        RegistrationState = new RegistrationInfo( registration, rockContext );
                        RegistrationState.PreviousPaymentTotal = registrationService.GetTotalPayments( registration.Id );
                    }
                    else
                    {
                        ShowError( "Sorry", "You are not allowed to view or edit the selected registration since you are not the one who created the registration." );
                        return false;
                    }

                    // set the max number of steps in the progress bar
                    numHowMany.Value = registration.Registrants.Count();
                    this.ProgressBarSteps = numHowMany.Value * FormCount + 2;

                    // set group id
                    if ( groupId.HasValue )
                    {
                        GroupId = groupId;
                    }
                    else if ( !string.IsNullOrWhiteSpace( registrationSlug ) )
                    {
                        var dateTime = RockDateTime.Now;
                        var linkage = new EventItemOccurrenceGroupMapService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( l =>
                                l.UrlSlug == registrationSlug &&
                                l.RegistrationInstance != null &&
                                l.RegistrationInstance.IsActive &&
                                l.RegistrationInstance.RegistrationTemplate != null &&
                                l.RegistrationInstance.RegistrationTemplate.IsActive &&
                                (!l.RegistrationInstance.StartDateTime.HasValue || l.RegistrationInstance.StartDateTime <= dateTime) &&
                                (!l.RegistrationInstance.EndDateTime.HasValue || l.RegistrationInstance.EndDateTime > dateTime) )
                            .FirstOrDefault();
                        if ( linkage != null )
                        {
                            GroupId = linkage.GroupId;
                        }
                    }
                }
                else
                {
                    ShowError( "Sorry", "You are not allowed to view or edit the selected registration since you are not the one who created the registration." );
                    return false;
                }
            }

            // A registration slug was specified
            if ( RegistrationState == null && !string.IsNullOrWhiteSpace( registrationSlug ) )
            {
                var dateTime = RockDateTime.Now;
                var linkage = new EventItemOccurrenceGroupMapService( rockContext )
                    .Queryable( "RegistrationInstance.Account,RegistrationInstance.RegistrationTemplate.Fees,RegistrationInstance.RegistrationTemplate.Discounts,RegistrationInstance.RegistrationTemplate.Forms.Fields.Attribute,RegistrationInstance.RegistrationTemplate.FinancialGateway" )
                    .Where( l => 
                        l.UrlSlug == registrationSlug &&
                        l.RegistrationInstance != null &&
                        l.RegistrationInstance.IsActive &&
                        l.RegistrationInstance.RegistrationTemplate != null &&
                        l.RegistrationInstance.RegistrationTemplate.IsActive &&
                        (!l.RegistrationInstance.StartDateTime.HasValue || l.RegistrationInstance.StartDateTime <= dateTime ) &&
                        (!l.RegistrationInstance.EndDateTime.HasValue || l.RegistrationInstance.EndDateTime > dateTime )  )
                    .FirstOrDefault();

                if ( linkage != null )
                {
                    RegistrationInstanceState = linkage.RegistrationInstance;
                    GroupId = linkage.GroupId;
                    RegistrationState = new RegistrationInfo( CurrentPerson );
                }
            }

            // A group id and campus id were specified
            if ( RegistrationState == null && groupId.HasValue && campusId.HasValue )
            {
                var dateTime = RockDateTime.Now;
                var linkage = new EventItemOccurrenceGroupMapService( rockContext )
                    .Queryable( "RegistrationInstance.Account,RegistrationInstance.RegistrationTemplate.Fees,RegistrationInstance.RegistrationTemplate.Discounts,RegistrationInstance.RegistrationTemplate.Forms.Fields.Attribute,RegistrationInstance.RegistrationTemplate.FinancialGateway" )
                    .Where( l =>
                        l.GroupId == groupId &&
                        l.EventItemOccurrence != null &&
                        l.EventItemOccurrence.CampusId == campusId &&
                        l.RegistrationInstance != null &&
                        l.RegistrationInstance.IsActive &&
                        l.RegistrationInstance.RegistrationTemplate != null &&
                        l.RegistrationInstance.RegistrationTemplate.IsActive &&
                        ( !l.RegistrationInstance.StartDateTime.HasValue || l.RegistrationInstance.StartDateTime <= dateTime ) &&
                        ( !l.RegistrationInstance.EndDateTime.HasValue || l.RegistrationInstance.EndDateTime > dateTime ) )
                    .FirstOrDefault();

                CampusId = campusId;
                if ( linkage != null )
                {
                    RegistrationInstanceState = linkage.RegistrationInstance;
                    GroupId = linkage.GroupId;
                    RegistrationState = new RegistrationInfo( CurrentPerson );
                }
            }

            // A registration instance id was specified
            if ( RegistrationState == null && registrationInstanceId.HasValue )
            {
                var dateTime = RockDateTime.Now;
                RegistrationInstanceState = new RegistrationInstanceService( rockContext )
                    .Queryable( "Account,RegistrationTemplate.Fees,RegistrationTemplate.Discounts,RegistrationTemplate.Forms.Fields.Attribute,RegistrationTemplate.FinancialGateway" )
                    .Where( r =>
                        r.Id == registrationInstanceId.Value &&
                        r.IsActive &&
                        r.RegistrationTemplate != null &&
                        r.RegistrationTemplate.IsActive &&
                        ( !r.StartDateTime.HasValue || r.StartDateTime <= dateTime ) &&
                        ( !r.EndDateTime.HasValue || r.EndDateTime > dateTime ) )
                    .FirstOrDefault();

                if ( RegistrationInstanceState != null )
                {
                    RegistrationState = new RegistrationInfo( CurrentPerson );
                }
            }

            // If registration instance id and event occurrence were specified, but a group (linkage) hasn't been loaded, find the first group for the event occurrence
            if ( RegistrationInstanceState != null && eventOccurrenceId.HasValue && !groupId.HasValue )
            {
                var eventItemOccurrence = new EventItemOccurrenceService( rockContext )
                    .Queryable()
                    .Where( o => o.Id == eventOccurrenceId.Value )
                    .FirstOrDefault();
                if ( eventItemOccurrence != null )
                {
                    CampusId = eventItemOccurrence.CampusId;

                    var linkage = eventItemOccurrence.Linkages
                        .Where( l => l.RegistrationInstanceId == RegistrationInstanceState.Id )
                        .FirstOrDefault();

                    if ( linkage != null )
                    {
                        GroupId = linkage.GroupId;
                    }
                }
            }

            if ( RegistrationState != null && 
                RegistrationState.FamilyGuid == Guid.Empty &&
                RegistrationTemplate != null && 
                RegistrationTemplate.RegistrantsSameFamily != RegistrantsSameFamily.Ask )
            {
                RegistrationState.FamilyGuid = Guid.NewGuid();
            }

            if ( RegistrationState != null && !RegistrationState.Registrants.Any() )
            {
                SetRegistrantState( 1 );
            }

            return true;
        }

        /// <summary>
        /// Adds (or removes) registrants to or from the registration. Only newly added registrants can
        /// can be removed. Any existing (saved) registrants cannot be removed from the registration
        /// </summary>
        /// <param name="registrantCount">The number of registrants that registration should have.</param>
        private void SetRegistrantState( int registrantCount )
        {
            if ( RegistrationState != null )
            {
                // If this is the first registrant being added, default it to the current person
                if ( RegistrationState.RegistrantCount == 0 && registrantCount == 1 && CurrentPerson != null )
                {
                    var registrant = new RegistrantInfo( RegistrationInstanceState, CurrentPerson );
                    registrant.Cost = RegistrationTemplate.Cost;
                    registrant.FamilyGuid = RegistrationState.FamilyGuid;
                    RegistrationState.Registrants.Add( registrant );
                }
                
                // While the number of registrants belonging to registration is less than the selected count, addd another registrant
                while ( RegistrationState.RegistrantCount < registrantCount )
                {
                    var registrant = new RegistrantInfo { Cost = RegistrationTemplate.Cost };
                    if ( RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.No )
                    {
                        registrant.FamilyGuid = Guid.NewGuid();
                    } 
                    else if ( RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Yes )
                    {
                        registrant.FamilyGuid = RegistrationState.FamilyGuid;
                    }

                    RegistrationState.Registrants.Add( registrant );
                }

                // Get the number of registrants that needs to be removed. 
                int removeCount = RegistrationState.RegistrantCount - registrantCount;
                if ( removeCount > 0 )
                {
                    // If removing any, reverse the order of registrants, so that most recently added will be removed first
                    RegistrationState.Registrants.Reverse();

                    // Try to get the registrants to remove. Most recently added will be taken first
                    foreach ( var registrant in RegistrationState.Registrants.Take( removeCount ).ToList() )
                    {
                        RegistrationState.Registrants.Remove( registrant );
                    }

                    // Reset the order after removing any registrants
                    RegistrationState.Registrants.Reverse();
                }
            }
        }

        #endregion

        #region Save Methods

        private List<string> ValidatePayment()
        {
            var validationErrors = new List<string>();

            if ( minimumPayment.HasValue && minimumPayment > 0.0M )
            {
                if ( RegistrationState.PaymentAmount < minimumPayment )
                {
                    validationErrors.Add( string.Format( "Amount To Pay Today must be at least {0:C2}", minimumPayment ) );
                }

                if ( RegistrationState.PaymentAmount > 0.0M )
                {
                    // If not using a saved account validate cc fields
                    if ( rblSavedCC.Items.Count == 0 || ( rblSavedCC.SelectedValueAsInt() ?? 0 ) == 0 )
                    {
                        if ( txtCardFirstName.Visible && string.IsNullOrWhiteSpace( txtCardFirstName.Text ) )
                        {
                            validationErrors.Add( "First Name on Card is required" );
                        }
                        if ( txtCardLastName.Visible && string.IsNullOrWhiteSpace( txtCardLastName.Text ) )
                        {
                            validationErrors.Add( "Last Name on Card is required" );
                        }
                        if ( txtCardName.Visible && string.IsNullOrWhiteSpace( txtCardName.Text ) )
                        {
                            validationErrors.Add( "Name on Card is required" );
                        }
                        if ( string.IsNullOrWhiteSpace( txtCreditCard.Text ) )
                        {
                            validationErrors.Add( "Credit Card # is required" );
                        }
                        if ( !mypExpiration.SelectedDate.HasValue )
                        {
                            validationErrors.Add( "Credit Card Expiration Date is required" );
                        }
                        if ( string.IsNullOrWhiteSpace( txtCVV.Text ) )
                        {
                            validationErrors.Add( "Credit Card Security Code is required" );
                        }
                        if ( string.IsNullOrWhiteSpace( acBillingAddress.Street1 ) ||
                            string.IsNullOrWhiteSpace( acBillingAddress.City ) ||
                            string.IsNullOrWhiteSpace( acBillingAddress.State ) ||
                            string.IsNullOrWhiteSpace( acBillingAddress.PostalCode ) )
                        {
                            validationErrors.Add( "Billing Address is required" );
                        }
                    }
                }
            }

            return validationErrors;
        }

        /// <summary>
        /// Saves the changes.
        /// </summary>
        /// <returns></returns>
        private int? SaveChanges()
        {
            if ( !string.IsNullOrWhiteSpace( TransactionCode ) )
            {
                ShowError( string.Empty, "You have already completed this " + RegistrationTerm.ToLower() );
                return null;
            }

            Registration registration = null;

            if ( RegistrationState != null && RegistrationState.Registrants.Any() && RegistrationTemplate != null )
            {
                var rockContext = new RockContext();

                var registrationService = new RegistrationService( rockContext );

                bool isNewRegistration = true;
                var previousRegistrants = new List<int>();
                if ( RegistrationState.RegistrationId.HasValue )
                {
                    var previousRegistration = registrationService.Get( RegistrationState.RegistrationId.Value );
                    if ( previousRegistration != null )
                    {
                        isNewRegistration = false;
                        previousRegistrants = previousRegistration.Registrants
                            .Where( r => r.PersonAlias != null )
                            .Select( r => r.PersonAlias.PersonId )
                            .ToList();
                    }
                }

                try
                {
                    bool hasPayment = ( RegistrationState.PaymentAmount ?? 0.0m ) > 0.0m;

                    // Save the registration
                    registration = SaveRegistration( rockContext, hasPayment );
                    if ( registration != null )
                    {
                        // If there is a payment being made, process the payment
                        if ( hasPayment )
                        {
                            string errorMessage = string.Empty;
                            if ( !ProcessPayment( rockContext, registration, out errorMessage ) )
                            {
                                throw new Exception( errorMessage );
                            }
                        }

                        try 
                        {
                            // If there is a valid registration, and nothing went wrong processing the payment, add registrants to group and send the notifications
                            if ( registration != null )
                            {
                                SavePersonNotes( rockContext, previousRegistrants, registration );
                                AddRegistrantsToGroup( rockContext, registration );

                                string appRoot = ResolveRockUrl( "~/" );
                                string themeRoot = ResolveRockUrl( "~~/" );

                                var confirmation = new Rock.Transactions.SendRegistrationConfirmationTransaction();
                                confirmation.RegistrationId = registration.Id;
                                confirmation.AppRoot = appRoot;
                                confirmation.ThemeRoot = themeRoot;
                                Rock.Transactions.RockQueue.TransactionQueue.Enqueue( confirmation );

                                var notification = new Rock.Transactions.SendRegistrationNotificationTransaction();
                                notification.RegistrationId = registration.Id;
                                notification.AppRoot = appRoot;
                                notification.ThemeRoot = themeRoot;
                                Rock.Transactions.RockQueue.TransactionQueue.Enqueue( notification );

                                var newRegistration = registrationService
                                    .Queryable( "Registrants.PersonAlias.Person,Registrants.GroupMember,RegistrationInstance.Account,RegistrationInstance.RegistrationTemplate.Fees,RegistrationInstance.RegistrationTemplate.Discounts,RegistrationInstance.RegistrationTemplate.Forms.Fields.Attribute,RegistrationInstance.RegistrationTemplate.FinancialGateway" )
                                    .Where( r => r.Id == registration.Id )
                                    .FirstOrDefault();
                                if ( newRegistration != null )
                                {
                                    RegistrationInstanceState = newRegistration.RegistrationInstance;
                                    RegistrationState = new RegistrationInfo( newRegistration, rockContext );
                                    RegistrationState.PreviousPaymentTotal = registrationService.GetTotalPayments( registration.Id );
                                }
                            }
                        }
                        catch ( Exception postSaveEx )
                        {
                            ShowWarning( "The following occurred after processing your " + RegistrationTerm, postSaveEx.Message );
                            ExceptionLogService.LogException( postSaveEx, Context, RockPage.PageId, RockPage.Layout.SiteId, CurrentPersonAlias );
                        }
                    }

                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex, Context, this.RockPage.PageId, this.RockPage.Site.Id, CurrentPersonAlias );

                    string message = ex.Message;
                    while ( ex.InnerException != null )
                    {
                        ex = ex.InnerException;
                        message = ex.Message;
                    }

                    ShowError( "An Error Occurred Processing Your " + RegistrationTerm, ex.Message );

                    // Try to delete the registration if it was just created
                    try
                    {
                        if ( isNewRegistration && registration != null && registration.Id > 0 )
                        {
                            RegistrationState.RegistrationId = null;
                            using ( var newRockContext = new RockContext() )
                            {
                                HistoryService.DeleteChanges( newRockContext, typeof( Registration ), registration.Id );

                                var newRegistrationService = new RegistrationService( newRockContext );
                                var newRegistration = newRegistrationService.Get( registration.Id );
                                if ( newRegistration != null )
                                {
                                    newRegistrationService.Delete( newRegistration );
                                    newRockContext.SaveChanges();
                                }
                            }
                        }
                    }
                    catch { }

                    return (int?)null;
                }
            }

            return registration != null ? registration.Id : (int?)null;
        }

        /// <summary>
        /// Saves the registration.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="hasPayment">if set to <c>true</c> [has payment].</param>
        /// <returns></returns>
        private Registration SaveRegistration( RockContext rockContext, bool hasPayment )
        {
            var registrationService = new RegistrationService( rockContext );
            var registrantService = new RegistrationRegistrantService( rockContext );
            var registrantFeeService = new RegistrationRegistrantFeeService( rockContext );
            var personService = new PersonService( rockContext );
            var groupService = new GroupService( rockContext );

            // variables to keep track of the family that new people should be added to
            int? singleFamilyId = null;
            var multipleFamilyGroupIds = new Dictionary<Guid, int>();

            var dvcConnectionStatus = DefinedValueCache.Read( GetAttributeValue( "ConnectionStatus" ).AsGuid() );
            var dvcRecordStatus = DefinedValueCache.Read( GetAttributeValue( "RecordStatus" ).AsGuid() );
            var familyGroupType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );
            var adultRoleId = familyGroupType.Roles
                .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ) )
                .Select( r => r.Id )
                .FirstOrDefault();
            var childRoleId = familyGroupType.Roles
                .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ) )
                .Select( r => r.Id )
                .FirstOrDefault();

            bool newRegistration = false;
            Registration registration = null;
            Person registrar = null;
            var registrationChanges = new List<string>();
            
            if ( RegistrationState.RegistrationId.HasValue )
            {
                registration = registrationService.Get( RegistrationState.RegistrationId.Value );
            }

            if ( registration == null )
            {
                newRegistration = true;
                registration = new Registration();
                registrationService.Add( registration );
                registrationChanges.Add( "Created Registration" );
            }
            else
            {
                if ( registration.PersonAlias != null && registration.PersonAlias.Person != null )
                {
                    registrar = registration.PersonAlias.Person;
                }
            }

            registration.RegistrationInstanceId = RegistrationInstanceState.Id;

            // If the Registration Instance linkage specified a group, load it now
            Group group = null;
            if ( GroupId.HasValue )
            {
                group = new GroupService( rockContext ).Get( GroupId.Value );
                if ( group != null && ( !registration.GroupId.HasValue || registration.GroupId.Value != group.Id ) )
                {
                    registration.GroupId = group.Id;
                    History.EvaluateChange( registrationChanges, "Group", string.Empty, group.Name );
                }
            }

            bool newRegistrar = newRegistration || 
                registration.FirstName == null || !registration.FirstName.Equals( RegistrationState.FirstName, StringComparison.OrdinalIgnoreCase ) ||
                registration.LastName == null || !registration.LastName.Equals( RegistrationState.LastName, StringComparison.OrdinalIgnoreCase );

            History.EvaluateChange( registrationChanges, "First Name", registration.FirstName, RegistrationState.FirstName );
            registration.FirstName = RegistrationState.FirstName;

            History.EvaluateChange( registrationChanges, "Last Name", registration.LastName, RegistrationState.LastName );
            registration.LastName = RegistrationState.LastName;

            History.EvaluateChange( registrationChanges, "Confirmation Email", registration.ConfirmationEmail, RegistrationState.ConfirmationEmail );
            registration.ConfirmationEmail = RegistrationState.ConfirmationEmail;

            History.EvaluateChange( registrationChanges, "Discount Code", registration.DiscountCode, RegistrationState.DiscountCode );
            registration.DiscountCode = RegistrationState.DiscountCode;

            History.EvaluateChange( registrationChanges, "Discount Percentage", registration.DiscountPercentage, RegistrationState.DiscountPercentage );
            registration.DiscountPercentage = RegistrationState.DiscountPercentage;

            History.EvaluateChange( registrationChanges, "Discount Amount", registration.DiscountAmount, RegistrationState.DiscountAmount );
            registration.DiscountAmount = RegistrationState.DiscountAmount;

            if ( newRegistrar )
            {
                // If the 'your name' value equals the currently logged in person, use their person alias id
                if ( CurrentPerson != null &&
                ( CurrentPerson.NickName.Trim().Equals( registration.FirstName.Trim(), StringComparison.OrdinalIgnoreCase ) ||
                    CurrentPerson.FirstName.Trim().Equals( registration.FirstName.Trim(), StringComparison.OrdinalIgnoreCase ) ) &&
                CurrentPerson.LastName.Trim().Equals( registration.LastName.Trim(), StringComparison.OrdinalIgnoreCase ) )
                {
                    registrar = CurrentPerson;
                    registration.PersonAliasId = CurrentPerson.PrimaryAliasId;
                }
                else
                {
                    // otherwise look for one and one-only match by name/email
                    var personMatches = personService.GetByMatch( registration.FirstName, registration.LastName, registration.ConfirmationEmail );
                    if ( personMatches.Count() == 1 )
                    {
                        registrar = personMatches.First();
                        registration.PersonAliasId = registrar.PrimaryAliasId;
                    }
                    else
                    {
                        registrar = null;
                        registration.PersonAlias = null;
                        registration.PersonAliasId = null;
                    }
                }
            }

            // Set the family guid for any other registrants that were selected to be in the same family
            if ( registrar != null )
            {
                var family = registrar.GetFamilies( rockContext ).FirstOrDefault();
                if ( family != null )
                {
                    multipleFamilyGroupIds.AddOrIgnore( RegistrationState.FamilyGuid, family.Id );
                    if ( !singleFamilyId.HasValue )
                    {
                        singleFamilyId = family.Id;
                    }
                }
            }

            // If the registration includes a payment, make sure there's an actual person associated to registration
            if ( hasPayment && !registration.PersonAliasId.HasValue )
            {
                // If a match was not found, create a new person
                var person = new Person();
                person.FirstName = registration.FirstName;
                person.LastName = registration.LastName;
                person.IsEmailActive = true;
                person.Email = registration.ConfirmationEmail;
                person.EmailPreference = EmailPreference.EmailAllowed;
                person.RecordTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                if ( dvcConnectionStatus != null )
                {
                    person.ConnectionStatusValueId = dvcConnectionStatus.Id;
                }
                if ( dvcRecordStatus != null )
                {
                    person.RecordStatusValueId = dvcRecordStatus.Id;
                }

                registrar = SavePerson( rockContext, person, RegistrationState.FamilyGuid, CampusId, null, adultRoleId, childRoleId, multipleFamilyGroupIds, ref singleFamilyId );
                registration.PersonAliasId = registrar != null ? registrar.PrimaryAliasId : (int?)null;

                History.EvaluateChange( registrationChanges, "Registrar", string.Empty, registrar.FullName );
            }
            else
            {
                if ( newRegistration )
                {
                    History.EvaluateChange( registrationChanges, "Registrar", string.Empty, registration.ToString() );
                }
            }

            // Save the registration ( so we can get an id )
            rockContext.SaveChanges();

            try
            {

                Task.Run( () =>
                    HistoryService.SaveChanges(
                        new RockContext(),
                        typeof( Registration ),
                        Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                        registration.Id,
                        registrationChanges, true, CurrentPersonAliasId )
                );


                // Get each registrant
                foreach ( var registrantInfo in RegistrationState.Registrants.ToList() )
                {
                    var registrantChanges = new List<string>();
                    var personChanges = new List<string>();
                    var familyChanges = new List<string>();

                    RegistrationRegistrant registrant = null;
                    Person person = null;

                    string firstName = registrantInfo.GetFirstName( RegistrationTemplate );
                    string lastName = registrantInfo.GetLastName( RegistrationTemplate );
                    string email = registrantInfo.GetEmail( RegistrationTemplate );

                    if ( registrantInfo.Id > 0 )
                    {
                        registrant = registration.Registrants.FirstOrDefault( r => r.Id == registrantInfo.Id );
                        if ( registrant != null )
                        {
                            person = registrant.Person;
                            if ( person != null && (
                                ( registrant.Person.FirstName.Equals( firstName, StringComparison.OrdinalIgnoreCase ) || registrant.Person.NickName.Equals( firstName, StringComparison.OrdinalIgnoreCase ) ) &&
                                registrant.Person.LastName.Equals( lastName, StringComparison.OrdinalIgnoreCase ) ) )
                            {
                                //
                            }
                            else
                            {
                                person = null;
                                registrant.PersonAlias = null;
                                registrant.PersonAliasId = null;
                            }
                        }
                    }

                    if ( person == null )
                    {
                        // Try to find a matching person based on name and email address
                        var personMatches = personService.GetByMatch( firstName, lastName, email );
                        if ( personMatches.Count() == 1 )
                        {
                            person = personMatches.First();
                        }

                        // Try to find a matching person based on name within same family as registrar
                        if ( person == null && registrar != null && registrantInfo.FamilyGuid == RegistrationState.FamilyGuid )
                        {
                            var familyMembers = registrar.GetFamilyMembers( true, rockContext )
                                .Where( m =>
                                    ( m.Person.FirstName == firstName || m.Person.NickName == firstName ) &&
                                    m.Person.LastName == lastName )
                                .Select( m => m.Person )
                                .ToList();

                            if ( familyMembers.Count() == 1 )
                            {
                                person = familyMembers.First();
                                if ( !string.IsNullOrWhiteSpace( email ) )
                                {
                                    person.Email = email;
                                }
                            }

                            if ( familyMembers.Count() > 1 && !string.IsNullOrWhiteSpace( email ) )
                            {
                                familyMembers = familyMembers
                                    .Where( m =>
                                        m.Email != null &&
                                        m.Email.Equals( email, StringComparison.OrdinalIgnoreCase ) )
                                    .ToList();
                                if ( familyMembers.Count() == 1 )
                                {
                                    person = familyMembers.First();
                                }
                            }
                        }
                    }

                    if ( person == null )
                    {
                        // If a match was not found, create a new person
                        person = new Person();
                        person.FirstName = firstName;
                        person.LastName = lastName;
                        person.IsEmailActive = true;
                        person.Email = email;
                        person.EmailPreference = EmailPreference.EmailAllowed;
                        person.RecordTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                        if ( dvcConnectionStatus != null )
                        {
                            person.ConnectionStatusValueId = dvcConnectionStatus.Id;
                        }

                        if ( dvcRecordStatus != null )
                        {
                            person.RecordStatusValueId = dvcRecordStatus.Id;
                        }
                    }

                    int? campusId = CampusId;
                    Location location = null;

                    // Set any of the template's person fields
                    foreach ( var field in RegistrationTemplate.Forms
                        .SelectMany( f => f.Fields
                            .Where( t => t.FieldSource == RegistrationFieldSource.PersonField ) ) )
                    {
                        // Find the registrant's value
                        var fieldValue = registrantInfo.FieldValues
                            .Where( f => f.Key == field.Id )
                            .Select( f => f.Value.FieldValue )
                            .FirstOrDefault();


                        if ( fieldValue != null )
                        {
                            switch ( field.PersonFieldType )
                            {
                                case RegistrationPersonFieldType.Campus:
                                    {
                                        if ( fieldValue != null )
                                        {
                                            campusId = fieldValue.ToString().AsIntegerOrNull();
                                        }
                                        break;
                                    }

                                case RegistrationPersonFieldType.Address:
                                    {
                                        location = fieldValue as Location;
                                        break;
                                    }

                                case RegistrationPersonFieldType.Birthdate:
                                    {
                                        var birthMonth = person.BirthMonth;
                                        var birthDay = person.BirthDay;
                                        var birthYear = person.BirthYear;

                                        person.SetBirthDate( fieldValue as DateTime? );

                                        History.EvaluateChange( personChanges, "Birth Month", birthMonth, person.BirthMonth );
                                        History.EvaluateChange( personChanges, "Birth Day", birthDay, person.BirthDay );
                                        History.EvaluateChange( personChanges, "Birth Year", birthYear, person.BirthYear );

                                        break;
                                    }

                                case RegistrationPersonFieldType.Gender:
                                    {
                                        var newGender = fieldValue.ToString().ConvertToEnumOrNull<Gender>() ?? Gender.Unknown;
                                        History.EvaluateChange( personChanges, "Gender", person.Gender, newGender );
                                        person.Gender = newGender;
                                        break;
                                    }

                                case RegistrationPersonFieldType.MaritalStatus:
                                    {
                                        if ( fieldValue != null )
                                        {
                                            int? newMaritalStatusId = fieldValue.ToString().AsIntegerOrNull();
                                            History.EvaluateChange( personChanges, "Marital Status", DefinedValueCache.GetName( person.MaritalStatusValueId ), DefinedValueCache.GetName( newMaritalStatusId ) );
                                            person.MaritalStatusValueId = newMaritalStatusId;
                                        }
                                        break;
                                    }

                                case RegistrationPersonFieldType.MobilePhone:
                                    {
                                        SavePhone( fieldValue, person, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid(), personChanges );
                                        break;
                                    }

                                case RegistrationPersonFieldType.HomePhone:
                                    {
                                        SavePhone( fieldValue, person, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid(), personChanges );
                                        break;
                                    }

                                case RegistrationPersonFieldType.WorkPhone:
                                    {
                                        SavePhone( fieldValue, person, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid(), personChanges );
                                        break;
                                    }
                            }
                        }
                    }

                    // Save the person ( and family if needed )
                    SavePerson( rockContext, person, registrantInfo.FamilyGuid, campusId, location, adultRoleId, childRoleId, multipleFamilyGroupIds, ref singleFamilyId );

                    // Load the person's attributes
                    person.LoadAttributes();

                    // Set any of the template's person fields
                    foreach ( var field in RegistrationTemplate.Forms
                        .SelectMany( f => f.Fields
                            .Where( t =>
                                t.FieldSource == RegistrationFieldSource.PersonAttribute &&
                                t.AttributeId.HasValue ) ) )
                    {
                        // Find the registrant's value
                        var fieldValue = registrantInfo.FieldValues
                            .Where( f => f.Key == field.Id )
                            .Select( f => f.Value.FieldValue )
                            .FirstOrDefault();

                        if ( fieldValue != null )
                        {
                            var attribute = AttributeCache.Read( field.AttributeId.Value );
                            if ( attribute != null )
                            {
                                string originalValue = person.GetAttributeValue( attribute.Key );
                                string newValue = fieldValue.ToString();
                                person.SetAttributeValue( attribute.Key, fieldValue.ToString() );

                                if ( ( originalValue ?? string.Empty ).Trim() != ( newValue ?? string.Empty ).Trim() )
                                {
                                    string formattedOriginalValue = string.Empty;
                                    if ( !string.IsNullOrWhiteSpace( originalValue ) )
                                    {
                                        formattedOriginalValue = attribute.FieldType.Field.FormatValue( null, originalValue, attribute.QualifierValues, false );
                                    }

                                    string formattedNewValue = string.Empty;
                                    if ( !string.IsNullOrWhiteSpace( newValue ) )
                                    {
                                        formattedNewValue = attribute.FieldType.Field.FormatValue( null, newValue, attribute.QualifierValues, false );
                                    }

                                    Helper.SaveAttributeValue( person, attribute, newValue, rockContext );
                                    History.EvaluateChange( personChanges, attribute.Name, formattedOriginalValue, formattedNewValue );

                                }
                            }
                        }
                    }

                    string registrantName = person.FullName + ": ";

                    personChanges.ForEach( c => registrantChanges.Add( c ) );

                    if ( registrant == null )
                    {
                        registrant = new RegistrationRegistrant();
                        registrant.Guid = registrantInfo.Guid;
                        registrantService.Add( registrant );
                        registrant.RegistrationId = registration.Id;
                    }

                    registrant.PersonAliasId = person.PrimaryAliasId;
                    registrant.Cost = registrantInfo.Cost;

                    // Remove fees
                    // Remove/delete any registrant fees that are no longer in UI with quantity 
                    foreach ( var dbFee in registrant.Fees.ToList() )
                    {
                        if ( !registrantInfo.FeeValues.Keys.Contains( dbFee.RegistrationTemplateFeeId ) ||
                            registrantInfo.FeeValues[dbFee.RegistrationTemplateFeeId] == null ||
                            !registrantInfo.FeeValues[dbFee.RegistrationTemplateFeeId]
                                .Any( f =>
                                    f.Option == dbFee.Option &&
                                    f.Quantity > 0 ) )
                        {
                            registrantChanges.Add( string.Format( "Removed '{0}' Fee (Quantity:{1:N0}, Cost:{2:C2}, Option:{3}",
                                dbFee.RegistrationTemplateFee.Name, dbFee.Quantity, dbFee.Cost, dbFee.Option ) );

                            registrant.Fees.Remove( dbFee );
                            registrantFeeService.Delete( dbFee );
                        }
                    }

                    // Add or Update fees
                    foreach ( var uiFee in registrantInfo.FeeValues.Where( f => f.Value != null ) )
                    {
                        foreach ( var uiFeeOption in uiFee.Value )
                        {
                            var dbFee = registrant.Fees
                                .Where( f =>
                                    f.RegistrationTemplateFeeId == uiFee.Key &&
                                    f.Option == uiFeeOption.Option )
                                .FirstOrDefault();

                            if ( dbFee == null )
                            {
                                dbFee = new RegistrationRegistrantFee();
                                dbFee.RegistrationTemplateFeeId = uiFee.Key;
                                dbFee.Option = uiFeeOption.Option;
                                registrant.Fees.Add( dbFee );
                            }

                            var templateFee = dbFee.RegistrationTemplateFee;
                            if ( templateFee == null )
                            {
                                templateFee = RegistrationTemplate.Fees.Where( f => f.Id == uiFee.Key ).FirstOrDefault();
                            }

                            string feeName = templateFee != null ? templateFee.Name : "Fee";
                            if ( !string.IsNullOrWhiteSpace( uiFeeOption.Option ) )
                            {
                                feeName = string.Format( "{0} ({1})", feeName, uiFeeOption.Option );
                            }

                            if ( dbFee.Id <= 0 )
                            {
                                registrantChanges.Add( feeName + " Fee Added" );
                            }

                            History.EvaluateChange( registrantChanges, feeName + " Quantity", dbFee.Quantity, uiFeeOption.Quantity );
                            dbFee.Quantity = uiFeeOption.Quantity;

                            History.EvaluateChange( registrantChanges, feeName + " Cost", dbFee.Cost, uiFeeOption.Cost );
                            dbFee.Cost = uiFeeOption.Cost;
                        }
                    }

                    rockContext.SaveChanges();

                    // Set any of the templat's registrant attributes
                    registrant.LoadAttributes();
                    foreach ( var field in RegistrationTemplate.Forms
                        .SelectMany( f => f.Fields
                            .Where( t =>
                                t.FieldSource == RegistrationFieldSource.RegistrationAttribute &&
                                t.AttributeId.HasValue ) ) )
                    {
                        // Find the registrant's value
                        var fieldValue = registrantInfo.FieldValues
                            .Where( f => f.Key == field.Id )
                            .Select( f => f.Value.FieldValue )
                            .FirstOrDefault();

                        if ( fieldValue != null )
                        {
                            var attribute = AttributeCache.Read( field.AttributeId.Value );
                            if ( attribute != null )
                            {
                                string originalValue = registrant.GetAttributeValue( attribute.Key );
                                string newValue = fieldValue.ToString();
                                registrant.SetAttributeValue( attribute.Key, fieldValue.ToString() );

                                if ( ( originalValue ?? string.Empty ).Trim() != ( newValue ?? string.Empty ).Trim() )
                                {
                                    string formattedOriginalValue = string.Empty;
                                    if ( !string.IsNullOrWhiteSpace( originalValue ) )
                                    {
                                        formattedOriginalValue = attribute.FieldType.Field.FormatValue( null, originalValue, attribute.QualifierValues, false );
                                    }

                                    string formattedNewValue = string.Empty;
                                    if ( !string.IsNullOrWhiteSpace( newValue ) )
                                    {
                                        formattedNewValue = attribute.FieldType.Field.FormatValue( null, newValue, attribute.QualifierValues, false );
                                    }

                                    Helper.SaveAttributeValue( registrant, attribute, newValue, rockContext );
                                    History.EvaluateChange( registrantChanges, attribute.Name, formattedOriginalValue, formattedNewValue );
                                }
                            }
                        }
                    }

                    Task.Run( () =>
                        HistoryService.SaveChanges(
                            new RockContext(),
                            typeof( Registration ),
                            Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                            registration.Id,
                            registrantChanges,
                            "Registrant: " + person.FullName,
                            null, null, true, CurrentPersonAliasId )
                    );

                    // Clear this registran't family guid so it's not updated again
                    registrantInfo.FamilyGuid = Guid.Empty;

                }

                rockContext.SaveChanges();
            }

            catch ( Exception ex )
            {
                try
                {
                    using ( var newRockContext = new RockContext() )
                    {
                        if ( newRegistration )
                        {
                            var newRegistrationService = new RegistrationService( newRockContext );
                            var savedRegistration = new RegistrationService( newRockContext ).Get( registration.Id );
                            if ( savedRegistration != null )
                            {
                                HistoryService.DeleteChanges( newRockContext, typeof( Registration ), savedRegistration.Id );

                                newRegistrationService.Delete( savedRegistration );
                                newRockContext.SaveChanges();
                            }
                        }
                    }
                }
                catch ( Exception ex1 )
                {
                    string test = ex1.Message;
                }

                throw ex;
            }

            return registration;

        }

        /// <summary>
        /// Saves the person.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="person">The person.</param>
        /// <param name="familyGuid">The family unique identifier.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="location">The location.</param>
        /// <param name="adultRoleId">The adult role identifier.</param>
        /// <param name="childRoleId">The child role identifier.</param>
        /// <param name="multipleFamilyGroupIds">The multiple family group ids.</param>
        /// <param name="singleFamilyId">The single family identifier.</param>
        /// <returns></returns>
        private Person SavePerson( RockContext rockContext, Person person, Guid familyGuid, int? campusId, Location location, int adultRoleId, int childRoleId,
            Dictionary<Guid, int> multipleFamilyGroupIds, ref int? singleFamilyId )
        {
            int? familyId = null;

            if ( person.Id > 0 )
            {
                rockContext.SaveChanges();

                // Set the family guid for any other registrants that were selected to be in the same family
                var family = person.GetFamilies( rockContext ).FirstOrDefault();
                if ( family != null )
                {
                    familyId = family.Id;
                    multipleFamilyGroupIds.AddOrIgnore( familyGuid, family.Id );
                    if ( !singleFamilyId.HasValue )
                    {
                        singleFamilyId = family.Id;
                    }
                }
            }
            else
            {
                // If we've created the family aready for this registrant, add them to it
                if (
                        ( RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Ask && multipleFamilyGroupIds.ContainsKey( familyGuid ) ) ||
                        ( RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Yes && singleFamilyId.HasValue )
                    )
                {

                    // Add person to existing family
                    var age = person.Age;
                    int familyRoleId = age.HasValue && age < 18 ? childRoleId : adultRoleId;

                    familyId = RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Ask ?
                        multipleFamilyGroupIds[familyGuid] :
                        singleFamilyId.Value;
                    PersonService.AddPersonToFamily( person, true, multipleFamilyGroupIds[familyGuid], familyRoleId, rockContext );

                }

                // otherwise create a new family
                else
                {
                    // Create Person/Family
                    var familyGroup = PersonService.SaveNewPerson( person, rockContext, campusId, false );
                    if ( familyGroup != null )
                    {
                        familyId = familyGroup.Id;

                        // Store the family id for next person 
                        multipleFamilyGroupIds.AddOrIgnore( familyGuid, familyGroup.Id );
                        if ( !singleFamilyId.HasValue )
                        {
                            singleFamilyId = familyGroup.Id;
                        }
                    }
                }
            }


            if ( familyId.HasValue && location != null )
            {
                var homeLocationType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() );
                if ( homeLocationType != null )
                {
                    var familyGroup = new GroupService( rockContext ).Get( familyId.Value );
                    if ( familyGroup != null )
                    {
                        GroupService.AddNewFamilyAddress(
                            rockContext,
                            familyGroup,
                            Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME,
                            location.Street1, location.Street2, location.City, location.State, location.PostalCode, location.Country, true );
                    }
                }
            }

            return new PersonService( rockContext ).Get( person.Id );
        }

        /// <summary>
        /// Saves the phone.
        /// </summary>
        /// <param name="fieldValue">The field value.</param>
        /// <param name="person">The person.</param>
        /// <param name="phoneTypeGuid">The phone type unique identifier.</param>
        /// <param name="changes">The changes.</param>
        private void SavePhone( object fieldValue, Person person, Guid phoneTypeGuid, List<string> changes )
        {
            var phoneNumber = fieldValue as PhoneNumber;
            if ( phoneNumber != null )
            {
                string cleanNumber = PhoneNumber.CleanNumber( phoneNumber.Number );
                if ( !string.IsNullOrWhiteSpace( cleanNumber ) )
                {
                    var numberType = DefinedValueCache.Read( phoneTypeGuid );
                    if ( numberType != null )
                    {
                        var phone = person.PhoneNumbers.FirstOrDefault( p => p.NumberTypeValueId == numberType.Id );
                        string oldPhoneNumber = string.Empty;
                        if ( phone == null )
                        {
                            phone = new PhoneNumber();
                            person.PhoneNumbers.Add( phone );
                            phone.NumberTypeValueId = numberType.Id;
                        }
                        else
                        {
                            oldPhoneNumber = phone.NumberFormattedWithCountryCode;
                        }
                        phone.CountryCode = PhoneNumber.CleanNumber( phoneNumber.CountryCode );
                        phone.Number = cleanNumber;

                        History.EvaluateChange(
                            changes,
                            string.Format( "{0} Phone", numberType.Value ),
                            oldPhoneNumber,
                            phoneNumber.NumberFormattedWithCountryCode );
                    }
                }
            }
        }

        /// <summary>
        /// Saves the person notes.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="previousRegistrants">The previous registrants.</param>
        /// <param name="registration">The registration.</param>
        private void SavePersonNotes( RockContext rockContext, List<int> previousRegistrants, Registration registration )
        {
            // Setup Note settings
            NoteTypeCache noteType = null;
            if ( RegistrationTemplate != null && RegistrationTemplate.AddPersonNote )
            {
                noteType = NoteTypeCache.Read( Rock.SystemGuid.NoteType.PERSON_EVENT_REGISTRATION.AsGuid() );
                if ( noteType != null )
                {
                    var noteService = new NoteService( rockContext );
                    var personAliasService = new PersonAliasService( rockContext );

                    Person registrar = null;
                    if ( registration.PersonAliasId.HasValue )
                    {
                        registrar = personAliasService.GetPerson( registration.PersonAliasId.Value );
                    }

                    var registrantNames = new List<string>();

                    // Get each registrant
                    foreach ( var registrantPersonAliasId in registration.Registrants
                        .Where( r => r.PersonAliasId.HasValue )
                        .Select( r => r.PersonAliasId.Value )
                        .ToList() )
                    {
                        var registrant = personAliasService.GetPerson( registrantPersonAliasId );
                        if ( registrant != null && !previousRegistrants.Contains( registrant.Id ) )
                        {
                            var noteText = new StringBuilder();
                            noteText.AppendFormat( "Registered for {0}", RegistrationInstanceState.Name);

                            if ( registrar != null && registrar.Id != registrant.Id )
                            {
                                noteText.AppendFormat( " by {0}", registrar.FullName );
                                registrantNames.Add( registrant.FullName );
                            }

                            if ( registrar == null && ( RegistrationState.FirstName != registrar.NickName || RegistrationState.LastName != registrar.LastName ) )
                            {
                                noteText.AppendFormat( " by {0}", RegistrationState.FirstName + " " + RegistrationState.LastName );
                            }

                            if ( noteText.Length > 0 )
                            {
                                var note = new Note();
                                note.NoteTypeId = noteType.Id;
                                note.IsSystem = false;
                                note.IsAlert = false;
                                note.IsPrivateNote = false;
                                note.EntityId = registrant.Id;
                                note.Caption = string.Empty;
                                note.Text = noteText.ToString();
                                noteService.Add( note );
                            }
                        }

                    }

                    if ( registrar != null && registrantNames.Any() )
                    {
                        string namesText = string.Empty;
                        if ( registrantNames.Count >= 2 )
                        {
                            int lessOne = registrantNames.Count - 1;
                            namesText = registrantNames.Take( lessOne ).ToList().AsDelimited( ", " ) +
                                " and " +
                                registrantNames.Skip( lessOne ).Take( 1 ).First() + " ";
                        }
                        else
                        {
                            namesText = registrantNames.First() + " ";
                        }

                        var note = new Note();
                        note.NoteTypeId = noteType.Id;
                        note.IsSystem = false;
                        note.IsAlert = false;
                        note.IsPrivateNote = false;
                        note.EntityId = registrar.Id;
                        note.Caption = string.Empty;
                        note.Text = string.Format( "Registered {0} for {1}", namesText, RegistrationInstanceState.Name );
                        noteService.Add( note );

                    }

                    rockContext.SaveChanges();
                }
            }

        }

        /// <summary>
        /// Adds the registrants to group.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="registration">The registration.</param>
        private void AddRegistrantsToGroup( RockContext rockContext, Registration registration )
        {
            // If the registration instance linkage specified a group to add registrant to, add them if they're not already
            // part of that group
            if ( registration.GroupId.HasValue )
            {
                var groupService = new GroupService( rockContext );
                var personAliasService = new PersonAliasService( rockContext );
                var groupMemberService = new GroupMemberService( rockContext );

                var group = groupService.Get( registration.GroupId.Value );
                if ( group != null )
                {
                    foreach ( var registrant in registration.Registrants.Where( r => r.PersonAliasId.HasValue ).ToList() )
                    {
                        var personAlias = personAliasService.Get( registrant.PersonAliasId.Value );
                        GroupMember groupMember = group.Members.Where( m => m.PersonId == personAlias.PersonId ).FirstOrDefault();
                        if ( groupMember == null )
                        {
                            groupMember = new GroupMember();
                            groupMemberService.Add( groupMember );
                            groupMember.GroupId = group.Id;
                            groupMember.PersonId = personAlias.PersonId;

                            if ( RegistrationTemplate.GroupTypeId.HasValue &&
                                RegistrationTemplate.GroupTypeId == group.GroupTypeId &&
                                RegistrationTemplate.GroupMemberRoleId.HasValue )
                            {
                                groupMember.GroupRoleId = RegistrationTemplate.GroupMemberRoleId.Value;
                                groupMember.GroupMemberStatus = RegistrationTemplate.GroupMemberStatus;
                            }
                            else
                            {
                                if ( group.GroupType.DefaultGroupRoleId.HasValue )
                                {
                                    groupMember.GroupRoleId = group.GroupType.DefaultGroupRoleId.Value;
                                }
                                else
                                {
                                    groupMember.GroupRoleId = group.GroupType.Roles.Select( r => r.Id ).FirstOrDefault();
                                }
                                groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                            }
                        }

                        rockContext.SaveChanges();

                        registrant.GroupMemberId = groupMember != null ? groupMember.Id : (int?)null;
                        rockContext.SaveChanges();

                        // Set any of the template's group member attributes 
                        groupMember.LoadAttributes();

                        var registrantInfo = RegistrationState.Registrants.FirstOrDefault( r => r.Guid == registrant.Guid );
                        if ( registrantInfo != null )
                        {
                            foreach ( var field in RegistrationTemplate.Forms
                                .SelectMany( f => f.Fields
                                    .Where( t =>
                                        t.FieldSource == RegistrationFieldSource.GroupMemberAttribute &&
                                        t.AttributeId.HasValue ) ) )
                            {
                                // Find the registrant's value
                                var fieldValue = registrantInfo.FieldValues
                                    .Where( f => f.Key == field.Id )
                                    .Select( f => f.Value.FieldValue )
                                    .FirstOrDefault();

                                if ( fieldValue != null )
                                {
                                    var attribute = AttributeCache.Read( field.AttributeId.Value );
                                    if ( attribute != null )
                                    {
                                        string originalValue = groupMember.GetAttributeValue( attribute.Key );
                                        string newValue = fieldValue.ToString();
                                        groupMember.SetAttributeValue( attribute.Key, fieldValue.ToString() );

                                        if ( ( originalValue ?? string.Empty ).Trim() != ( newValue ?? string.Empty ).Trim() )
                                        {
                                            string formattedOriginalValue = string.Empty;
                                            if ( !string.IsNullOrWhiteSpace( originalValue ) )
                                            {
                                                formattedOriginalValue = attribute.FieldType.Field.FormatValue( null, originalValue, attribute.QualifierValues, false );
                                            }

                                            string formattedNewValue = string.Empty;
                                            if ( !string.IsNullOrWhiteSpace( newValue ) )
                                            {
                                                formattedNewValue = attribute.FieldType.Field.FormatValue( null, newValue, attribute.QualifierValues, false );
                                            }

                                            Helper.SaveAttributeValue( groupMember, attribute, newValue, rockContext );
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Processes the payment.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="registration">The registration.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private bool ProcessPayment( RockContext rockContext, Registration registration, out string errorMessage )
        {
            GatewayComponent gateway = null;
            if ( RegistrationTemplate != null && RegistrationTemplate.FinancialGateway != null )
            {
                gateway = RegistrationTemplate.FinancialGateway.GetGatewayComponent();
            }

            if ( gateway == null )
            {
                errorMessage = "There was a problem creating the payment gateway information";
                return false;
            }

            if ( !RegistrationInstanceState.AccountId.HasValue || RegistrationInstanceState.Account == null )
            {
                errorMessage = "There was a problem with the account configuration for this " + RegistrationTerm.ToLower();
                return false;
            }

            PaymentInfo paymentInfo = null;
            if ( rblSavedCC.Items.Count > 0 && ( rblSavedCC.SelectedValueAsId() ?? 0 ) > 0 )
            {
                var savedAccount = new FinancialPersonSavedAccountService( new RockContext() ).Get( rblSavedCC.SelectedValueAsId().Value );
                if ( savedAccount != null )
                {
                    paymentInfo = savedAccount.GetReferencePayment();
                    paymentInfo.Amount = RegistrationState.PaymentAmount ?? 0.0m;
                }
                else
                {
                    errorMessage = "There was a problem retrieving the saved account";
                    return false;
                }
            }
            else
            {
                paymentInfo = GetCCPaymentInfo( gateway );
            }

            paymentInfo.Comment1 = string.Format( "{0} ({1})", RegistrationInstanceState.Name, RegistrationInstanceState.Account.GlCode );

            var transaction = gateway.Charge( RegistrationTemplate.FinancialGateway, paymentInfo, out errorMessage );
            if ( transaction != null )
            {
                var txnChanges = new List<string>();
                txnChanges.Add( "Created Transaction" );

                History.EvaluateChange( txnChanges, "Transaction Code", string.Empty, transaction.TransactionCode );

                transaction.AuthorizedPersonAliasId = registration.PersonAliasId;

                transaction.TransactionDateTime = RockDateTime.Now;
                History.EvaluateChange( txnChanges, "Date/Time", null, transaction.TransactionDateTime );

                transaction.FinancialGatewayId = RegistrationTemplate.FinancialGatewayId;
                History.EvaluateChange( txnChanges, "Gateway", string.Empty, RegistrationTemplate.FinancialGateway.Name );

                var txnType = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_EVENT_REGISTRATION ) );
                transaction.TransactionTypeValueId = txnType.Id;
                History.EvaluateChange( txnChanges, "Type", string.Empty, txnType.Value );

                if ( transaction.FinancialPaymentDetail == null )
                {
                    transaction.FinancialPaymentDetail = new FinancialPaymentDetail();
                }
                transaction.FinancialPaymentDetail.SetFromPaymentInfo( paymentInfo, gateway, rockContext, txnChanges );

                Guid sourceGuid = Guid.Empty;
                if ( Guid.TryParse( GetAttributeValue( "Source" ), out sourceGuid ) )
                {
                    var source = DefinedValueCache.Read( sourceGuid );
                    if ( source != null )
                    {
                        transaction.SourceTypeValueId = source.Id;
                        History.EvaluateChange( txnChanges, "Source", string.Empty, source.Value );
                    }
                }

                transaction.Summary = registration.GetSummary( RegistrationInstanceState );

                var transactionDetail = new FinancialTransactionDetail();
                transactionDetail.Amount = RegistrationState.PaymentAmount ?? 0.0m;
                transactionDetail.AccountId = RegistrationInstanceState.AccountId.Value;
                transactionDetail.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Registration ) ).Id;
                transactionDetail.EntityId = registration.Id;
                transaction.TransactionDetails.Add( transactionDetail );

                History.EvaluateChange( txnChanges, RegistrationInstanceState.Account.Name, 0.0M.FormatAsCurrency(), transactionDetail.Amount.FormatAsCurrency() );

                var batchChanges = new List<string>();

                rockContext.WrapTransaction( () =>
                {
                    var batchService = new FinancialBatchService( rockContext );

                    // Get the batch
                    var batch = batchService.Get(
                        GetAttributeValue( "BatchNamePrefix" ),
                        paymentInfo.CurrencyTypeValue,
                        paymentInfo.CreditCardTypeValue,
                        transaction.TransactionDateTime.Value,
                        RegistrationTemplate.FinancialGateway.GetBatchTimeOffset() );

                    if ( batch.Id == 0 )
                    {
                        batchChanges.Add( "Generated the batch" );
                        History.EvaluateChange( batchChanges, "Batch Name", string.Empty, batch.Name );
                        History.EvaluateChange( batchChanges, "Status", null, batch.Status );
                        History.EvaluateChange( batchChanges, "Start Date/Time", null, batch.BatchStartDateTime );
                        History.EvaluateChange( batchChanges, "End Date/Time", null, batch.BatchEndDateTime );
                    }

                    decimal newControlAmount = batch.ControlAmount + transaction.TotalAmount;
                    History.EvaluateChange( batchChanges, "Control Amount", batch.ControlAmount.FormatAsCurrency(), newControlAmount.FormatAsCurrency() );
                    batch.ControlAmount = newControlAmount;

                    transaction.BatchId = batch.Id;
                    batch.Transactions.Add( transaction );

                    rockContext.SaveChanges();
                } );

                if ( transaction.BatchId.HasValue )
                {
                    Task.Run( () =>
                        HistoryService.SaveChanges(
                            new RockContext(),
                            typeof( FinancialBatch ),
                            Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
                            transaction.BatchId.Value,
                            batchChanges, true, CurrentPersonAliasId )
                    );

                    Task.Run( () =>
                        HistoryService.SaveChanges(
                            new RockContext(),
                            typeof( FinancialBatch ),
                            Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(),
                            transaction.BatchId.Value,
                            txnChanges,
                            CurrentPerson != null ? CurrentPerson.FullName : string.Empty,
                            typeof( FinancialTransaction ),
                            transaction.Id, true, CurrentPersonAliasId )
                    );
                }

                List<string> registrationChanges = new List<string>();
                registrationChanges.Add( string.Format( "Made {0} payment", transaction.TotalAmount.FormatAsCurrency() ) );
                Task.Run( () =>
                    HistoryService.SaveChanges(
                        new RockContext(),
                        typeof( Registration ),
                        Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                        registration.Id,
                        registrationChanges, true, CurrentPersonAliasId )
                );

                TransactionCode = transaction.TransactionCode;

                return true;
            }
            else
            {
                return false;
            }

        }

        private CreditCardPaymentInfo GetCCPaymentInfo( GatewayComponent gateway )
        {
            var ccPaymentInfo = new CreditCardPaymentInfo( txtCreditCard.Text, txtCVV.Text, mypExpiration.SelectedDate.Value );

            ccPaymentInfo.NameOnCard = gateway != null && gateway.SplitNameOnCard ? txtCardFirstName.Text : txtCardName.Text;
            ccPaymentInfo.LastNameOnCard = txtCardLastName.Text;

            ccPaymentInfo.BillingStreet1 = acBillingAddress.Street1;
            ccPaymentInfo.BillingStreet2 = acBillingAddress.Street2;
            ccPaymentInfo.BillingCity = acBillingAddress.City;
            ccPaymentInfo.BillingState = acBillingAddress.State;
            ccPaymentInfo.BillingPostalCode = acBillingAddress.PostalCode;
            ccPaymentInfo.BillingCountry = acBillingAddress.Country;

            ccPaymentInfo.Amount = RegistrationState.PaymentAmount ?? 0.0m;
            ccPaymentInfo.Email = RegistrationState.ConfirmationEmail;

            ccPaymentInfo.FirstName = RegistrationState.FirstName;
            ccPaymentInfo.LastName = RegistrationState.LastName;

            return ccPaymentInfo;
        }

        #endregion

        #region Display Methods

        /// <summary>
        /// Shows the how many panel
        /// </summary>
        private void ShowHowMany()
        {
            lRegistrantTerm.Text =RegistrantTerm.Pluralize().ToLower();

            // If this is an existing registration, go directly to the summary
            if ( RegistrationState != null && RegistrationState.RegistrationId.HasValue && !PageParameter( START_AT_BEGINNING ).AsBoolean() )
            {
                ShowSummary();
            }
            else
            {
                if ( MaxRegistrants > MinRegistrants )
                {
                    // If registration allows multiple registrants show the 'How Many' panel
                    numHowMany.Maximum = MaxRegistrants;
                    numHowMany.Minimum = MinRegistrants;
                    numHowMany.Value = RegistrationState != null ? RegistrationState.RegistrantCount : 1;

                    lbRegistrantPrev.Visible = true;

                    SetPanel( 0 );
                }
                else
                {
                    // ... else skip to the registrant panel
                    CurrentRegistrantIndex = 0;
                    CurrentFormIndex = 0;

                    SetRegistrantState( MinRegistrants );

                    lbRegistrantPrev.Visible = false;

                    ShowRegistrant();
                }
            }
        }

        /// <summary>
        /// Shows the registrant panel
        /// </summary>
        private void ShowRegistrant()
        {
            if ( RegistrationState != null && RegistrationState.RegistrantCount > 0 )
            {
                string title = RegistrationState.RegistrantCount <= 1 ? 
                    RegistrantTerm : 
                    ( CurrentRegistrantIndex + 1 ).ToOrdinalWords().Humanize( LetterCasing.Title ) + " " + RegistrantTerm;

                if ( CurrentFormIndex > 0 )
                {
                    title += " (cont)";
                }
                lRegistrantTitle.Text = title;

                pnlFamilyOptions.Visible = 
                    CurrentRegistrantIndex > 0 && 
                    RegistrationTemplate != null && 
                    RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Ask;

                pnlProgressBar.Visible = GetAttributeValue("DisplayProgressBar").AsBoolean();

                SetPanel( 1 );
            }
        }

        /// <summary>
        /// Shows the summary panel
        /// </summary>
        private void ShowSummary()
        {
            lRegistrationTerm.Text = RegistrationTemplate.RegistrationTerm;
            lRegistrationTerm2.Text = RegistrationTemplate.RegistrationTerm;
            
            SetPanel( 2 );
        }

        /// <summary>
        /// Shows the success panel
        /// </summary>
        private void ShowSuccess( int registrationId )
        {
            lSuccessTitle.Text = "Congratulations";
            lSuccess.Text = "You have successfully completed this registration.";

            try
            {
                using ( var rockContext = new RockContext() )
                {
                    var registration = new RegistrationService( rockContext )
                        .Queryable( "RegistrationInstance.RegistrationTemplate" )
                        .FirstOrDefault( r => r.Id == registrationId );

                    if ( registration != null &&
                        registration.RegistrationInstance != null &&
                        registration.RegistrationInstance.RegistrationTemplate != null )
                    {
                        var template = registration.RegistrationInstance.RegistrationTemplate;

                        var mergeFields = new Dictionary<string, object>();
                        mergeFields.Add( "CurrentPerson", CurrentPerson );
                        mergeFields.Add( "RegistrationInstance", registration.RegistrationInstance );
                        mergeFields.Add( "Registration", registration );

                        if ( template != null && !string.IsNullOrWhiteSpace( template.SuccessTitle ) )
                        {
                            lSuccessTitle.Text = template.SuccessTitle.ResolveMergeFields( mergeFields );
                        }
                        else
                        {
                            lSuccessTitle.Text = "Congratulations";
                        }

                        if ( template != null && !string.IsNullOrWhiteSpace( template.SuccessText ) )
                        {
                            lSuccess.Text = template.SuccessText.ResolveMergeFields( mergeFields );
                        }
                        else
                        {
                            lSuccess.Text = "You have successfully completed this " + RegistrationTerm.ToLower();
                        }

                        // show debug info
                        if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && UserCanEdit )
                        {
                            lSuccessDebug.Visible = true;
                            lSuccessDebug.Text = mergeFields.lavaDebugInfo();
                        }

                    }

                    if ( nbAmountPaid.Visible = true && 
                        nbAmountPaid.Text.AsDecimalOrNull().HasValue && 
                        nbAmountPaid.Text.AsDecimalOrNull().Value > 0.0M &&
                        ( rblSavedCC.Items.Count == 0 || ( rblSavedCC.SelectedValueAsId() ?? 0 ) == 0 ) )
                    {
                        cbSaveAccount.Visible = true;
                        pnlSaveAccount.Visible = true;
                        txtSaveAccount.Visible = true;

                        // If current person does not have a login, have them create a username and password
                        phCreateLogin.Visible = !new UserLoginService( rockContext ).GetByPersonId( CurrentPersonId ).Any();
                    }
                    else
                    {
                        pnlSaveAccount.Visible = false;
                    }
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, Context, this.RockPage.PageId, this.RockPage.Site.Id, CurrentPersonAlias );
            }

            SetPanel( 3 );
        }

        /// <summary>
        /// Creates the dynamic controls, and shows correct panel
        /// </summary>
        /// <param name="currentPanel">The current panel.</param>
        private void SetPanel( int currentPanel )
        {
            CurrentPanel = currentPanel;

            CreateDynamicControls( true );

            pnlHowMany.Visible = CurrentPanel <= 0;
            pnlRegistrant.Visible = CurrentPanel == 1;
            pnlSummaryAndPayment.Visible = CurrentPanel == 2;
            pnlSuccess.Visible = CurrentPanel == 3;
        }

        /// <summary>
        /// Shows a warning message.
        /// </summary>
        /// <param name="heading">The heading.</param>
        /// <param name="text">The text.</param>
        private void ShowWarning( string heading, string text )
        {
            nbMain.Heading = heading;
            nbMain.Text = string.Format( "<p>{0}</p>", text );
            nbMain.NotificationBoxType = NotificationBoxType.Warning;
            nbMain.Visible = true;
        }

        /// <summary>
        /// Shows an error message.
        /// </summary>
        /// <param name="heading">The heading.</param>
        /// <param name="text">The text.</param>
        private void ShowError( string heading, string text )
        {
            nbMain.Heading = heading;
            nbMain.Text = string.Format( "<p>{0}</p>", text );
            nbMain.NotificationBoxType = NotificationBoxType.Danger;
            nbMain.Visible = true;
        }

        /// <summary>
        /// Registers the client script.
        /// </summary>
        private void RegisterClientScript()
        {
            RockPage.AddScriptLink( ResolveUrl( "~/Scripts/jquery.creditCardTypeDetector.js" ) );

            string script = string.Format( @"
    // Adjust the label of 'is in the same family' based on value of first name entered
    $('input.js-first-name').change( function() {{
        var name = $(this).val();
        if ( name == null || name == '') {{
            name = 'Individual';
        }}
        var $lbl = $('div.js-registration-same-family').find('label.control-label')
        $lbl.text( name + ' is in the same family as');
    }} );

    $('#{0}').on('change', function() {{

        var totalCost = Number($('#{1}').val());
        var minDue = Number($('#{2}').val());
        var previouslyPaid = Number($('#{3}').val());
        var balanceDue = totalCost - previouslyPaid;

        // Format and validate the amount entered
        var amountPaid = minDue;
        var amountValue = $(this).val();
        if ( amountValue != null && amountValue != '' && !isNaN( amountValue ) ) {{
            amountPaid = Number( amountValue );
            if ( amountPaid < minDue ) {{
                amountPaid = minDue;
            }}
            if ( amountPaid > balanceDue ) {{
                amountPaid = balanceDue
            }}
        }}
        $(this).val(amountPaid.toFixed(2));

        var amountRemaining = totalCost - ( previouslyPaid + amountPaid );
        $('#{4}').text( '{6}' + amountRemaining.toFixed(2) );
        
    }});

    // Detect credit card type
    $('.credit-card').creditCardTypeDetector({{ 'credit_card_logos': '.card-logos' }});

    // Toggle credit card display if saved card option is available
    $('div.radio-content').prev('.form-group').find('input:radio').unbind('click').on('click', function () {{
        var $content = $(this).parents('div.form-group:first').next('.radio-content')
        var radioDisplay = $content.css('display');
        if ($(this).val() == 0 && radioDisplay == 'none') {{
            $content.slideToggle();
        }}
        else if ($(this).val() != 0 && radioDisplay != 'none') {{
            $content.slideToggle();
        }}
    }});

    // Hide or show a div based on selection of checkbox
    $('input:checkbox.toggle-input').unbind('click').on('click', function () {{
        $(this).parents('.checkbox').next('.toggle-content').slideToggle();
    }});

    if ( $('#{5}').val() == 'true' ) {{
        setTimeout('window.scrollTo(0,0)',0);
        $('#{5}').val('')
    }}
",
            nbAmountPaid.ClientID, // {0}
            hfTotalCost.ClientID, // {1}
            hfMinimumDue.ClientID, // {2}
            hfPreviouslyPaid.ClientID, // {3}
            lRemainingDue.ClientID, // {4}
            hfTriggerScroll.ClientID, // {5}
            GlobalAttributesCache.Value( "CurrencySymbol" ) // {6}
            );

            ScriptManager.RegisterStartupScript( Page, Page.GetType(), "registrationEntry", script, true );
        }

        #endregion

        #region Dynamic Control Methods

        /// <summary>
        /// Creates the dynamic controls fore each panel
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void CreateDynamicControls( bool setValues )
        {
            switch( CurrentPanel )
            {
                case 1:
                    CreateRegistrantControls( setValues );
                    break;
                case 2:
                    CreateSummaryControls( setValues );
                    break;
            }
        }

        /// <summary>
        /// Parses the dynamic controls.
        /// </summary>
        private void ParseDynamicControls()
        {
            switch ( CurrentPanel )
            {
                case 1:
                    ParseRegistrantControls();
                    break;
                case 2:
                    ParseSummaryControls();
                    break;
            }
        }

        #region Registrant Controls

        /// <summary>
        /// Creates the registrant controls.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void CreateRegistrantControls( bool setValues )
        {
            lRegistrantFeeCaption.Text = FeeTerm.Pluralize();

            phRegistrantControls.Controls.Clear();
            phFees.Controls.Clear();

            if ( FormCount > CurrentFormIndex )
            {
                // Get the current and previous registrant ( previous is used when a field has the 'IsSharedValue' property )
                // so that current registrant can use the previous registrants value
                RegistrantInfo registrant = null;
                RegistrantInfo previousRegistrant = null;

                if ( RegistrationState != null && RegistrationState.RegistrantCount > CurrentRegistrantIndex )
                {
                    registrant = RegistrationState.Registrants[CurrentRegistrantIndex];

                    // If this is not the first person, then check to see if option for asking about family should be displayed
                    if ( CurrentFormIndex == 0 && CurrentRegistrantIndex > 0 &&
                        RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Ask )
                    {
                        var familyOptions = RegistrationState.GetFamilyOptions( RegistrationTemplate, CurrentRegistrantIndex );
                        if ( familyOptions.Any() )
                        {
                            familyOptions.Add( familyOptions.ContainsKey( registrant.FamilyGuid ) ? 
                                Guid.NewGuid() : 
                                registrant.FamilyGuid.Equals( Guid.Empty ) ? Guid.NewGuid() : registrant.FamilyGuid,
                                "None of the above" );
                            rblFamilyOptions.DataSource = familyOptions;
                            rblFamilyOptions.DataBind();
                            rblFamilyOptions.Visible = true;
                        }
                        else
                        {
                            rblFamilyOptions.Visible = false;
                        }
                    }
                    else
                    {
                        rblFamilyOptions.Visible = false;
                    }

                    if ( setValues )
                    {
                        if ( CurrentRegistrantIndex > 0 )
                        {
                            previousRegistrant = RegistrationState.Registrants[CurrentRegistrantIndex - 1];
                        }

                        rblFamilyOptions.SetValue( registrant.FamilyGuid.ToString() );
                    }
                }

                var form = RegistrationTemplate.Forms.OrderBy( f => f.Order ).ToList()[CurrentFormIndex];
                foreach ( var field in form.Fields.Where( f => !f.IsInternal ).OrderBy( f => f.Order ) )
                {
                    object value = null;
                    if ( registrant != null && registrant.FieldValues.ContainsKey( field.Id ) )
                    {
                        value = registrant.FieldValues[field.Id].FieldValue;
                    }

                    if ( value == null && field.IsSharedValue && previousRegistrant != null && previousRegistrant.FieldValues.ContainsKey( field.Id ) )
                    {
                        value = previousRegistrant.FieldValues[field.Id].FieldValue;
                    } 
                    
                    if ( !string.IsNullOrWhiteSpace( field.PreText ) )
                    {
                        phRegistrantControls.Controls.Add( new LiteralControl( field.PreText ) );
                    }

                    if ( field.FieldSource == RegistrationFieldSource.PersonField )
                    {
                        CreatePersonField( field, setValues, value);
                    }
                    else
                    {
                        CreateAttributeField( field, setValues, value );
                    }

                    if ( !string.IsNullOrWhiteSpace( field.PostText ) )
                    {
                        phRegistrantControls.Controls.Add( new LiteralControl( field.PostText ) );
                    }

                }

                // If the current form, is the last one, add any fee controls
                if ( FormCount - 1 == CurrentFormIndex )
                {
                    foreach ( var fee in RegistrationTemplate.Fees )
                    {
                        var feeValues = new List<FeeInfo>();
                        if ( registrant != null && registrant.FeeValues.ContainsKey( fee.Id ) )
                        {
                            feeValues = registrant.FeeValues[fee.Id];
                        }
                        CreateFeeField( fee, setValues, feeValues );
                    }
                }
            }

            divFees.Visible = phFees.Controls.Count > 0;
        }

        /// <summary>
        /// Creates the person field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="setValue">if set to <c>true</c> [set value].</param>
        /// <param name="fieldValue">The field value.</param>
        private void CreatePersonField( RegistrationTemplateFormField field, bool setValue, object fieldValue )
        {

            switch ( field.PersonFieldType )
            {
                case RegistrationPersonFieldType.FirstName:
                    {
                        var tbFirstName = new RockTextBox();
                        tbFirstName.ID = "tbFirstName";
                        tbFirstName.Label = "First Name";
                        tbFirstName.Required = field.IsRequired;
                        tbFirstName.ValidationGroup = BlockValidationGroup;
                        tbFirstName.AddCssClass( "js-first-name" );
                        phRegistrantControls.Controls.Add( tbFirstName );

                        if ( setValue && fieldValue != null )
                        {
                            tbFirstName.Text = fieldValue.ToString();
                        }

                        break;
                    }

                case RegistrationPersonFieldType.LastName:
                    {
                        var tbLastName = new RockTextBox();
                        tbLastName.ID = "tbLastName";
                        tbLastName.Label = "Last Name";
                        tbLastName.Required = field.IsRequired;
                        tbLastName.ValidationGroup = BlockValidationGroup;
                        phRegistrantControls.Controls.Add( tbLastName );

                        if ( setValue && fieldValue != null )
                        {
                            tbLastName.Text = fieldValue.ToString();
                        }

                        break;
                    }

                case RegistrationPersonFieldType.Campus:
                    {
                        var cpHomeCampus = new CampusPicker();
                        cpHomeCampus.ID = "cpHomeCampus";
                        cpHomeCampus.Label = "Campus";
                        cpHomeCampus.Required = field.IsRequired;
                        cpHomeCampus.ValidationGroup = BlockValidationGroup;
                        cpHomeCampus.Campuses = CampusCache.All();

                        phRegistrantControls.Controls.Add( cpHomeCampus );

                        if ( setValue && fieldValue != null )
                        {
                            cpHomeCampus.SelectedCampusId = fieldValue.ToString().AsIntegerOrNull();
                        }
                        break;
                    }

                case RegistrationPersonFieldType.Address:
                    {
                        var acAddress = new AddressControl();
                        acAddress.ID = "acAddress";
                        acAddress.Label = "Address";
                        acAddress.UseStateAbbreviation = true;
                        acAddress.UseCountryAbbreviation = false;
                        acAddress.Required = field.IsRequired;
                        acAddress.ValidationGroup = BlockValidationGroup;

                        phRegistrantControls.Controls.Add( acAddress );

                        if ( setValue && fieldValue != null )
                        {
                            var value = fieldValue as Location;
                            acAddress.SetValues( value );
                        }

                        break;
                    }

                case RegistrationPersonFieldType.Email:
                    {
                        var tbEmail = new EmailBox();
                        tbEmail.ID = "tbEmail";
                        tbEmail.Label = "Email";
                        tbEmail.Required = field.IsRequired;
                        tbEmail.ValidationGroup = BlockValidationGroup;
                        phRegistrantControls.Controls.Add( tbEmail );

                        if ( setValue && fieldValue != null )
                        {
                            tbEmail.Text = fieldValue.ToString();
                        }

                        break;
                    }

                case RegistrationPersonFieldType.Birthdate:
                    {
                        var bpBirthday = new BirthdayPicker();
                        bpBirthday.ID = "bpBirthday";
                        bpBirthday.Label = "Birthday";
                        bpBirthday.Required = field.IsRequired;
                        bpBirthday.ValidationGroup = BlockValidationGroup;
                        phRegistrantControls.Controls.Add( bpBirthday );

                        if ( setValue && fieldValue != null )
                        {
                            var value = fieldValue as DateTime?;
                            bpBirthday.SelectedDate = value;
                        }

                        break;
                    }

                case RegistrationPersonFieldType.Gender:
                    {
                        var ddlGender = new RockDropDownList();
                        ddlGender.ID = "ddlGender";
                        ddlGender.Label = "Gender";
                        ddlGender.Required = field.IsRequired;
                        ddlGender.ValidationGroup = BlockValidationGroup;
                        ddlGender.BindToEnum<Gender>( false );

                        // change the 'Unknow' value to be blank instead
                        ddlGender.Items.FindByValue( "0" ).Text = string.Empty;

                        phRegistrantControls.Controls.Add( ddlGender );

                        if ( setValue && fieldValue != null )
                        {
                            var value = fieldValue.ToString().ConvertToEnumOrNull<Gender>() ?? Gender.Unknown;
                            ddlGender.SetValue( value.ConvertToInt() );
                        }

                        break;
                    }

                case RegistrationPersonFieldType.MaritalStatus:
                    {
                        var ddlMaritalStatus = new RockDropDownList();
                        ddlMaritalStatus.ID = "ddlMaritalStatus";
                        ddlMaritalStatus.Label = "Marital Status";
                        ddlMaritalStatus.Required = field.IsRequired;
                        ddlMaritalStatus.ValidationGroup = BlockValidationGroup;
                        ddlMaritalStatus.BindToDefinedType( DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS.AsGuid() ), true );
                        phRegistrantControls.Controls.Add( ddlMaritalStatus );

                        if ( setValue && fieldValue != null )
                        {
                            var value = fieldValue.ToString().AsInteger();
                            ddlMaritalStatus.SetValue( value );
                        }

                        break;
                    }

                case RegistrationPersonFieldType.MobilePhone:
                    {
                        var dv = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );
                        if ( dv != null )
                        {
                            var ppMobile = new PhoneNumberBox();
                            ppMobile.ID = "ppMobile";
                            ppMobile.Label = dv.Value;
                            ppMobile.Required = field.IsRequired;
                            ppMobile.ValidationGroup = BlockValidationGroup;
                            ppMobile.CountryCode = PhoneNumber.DefaultCountryCode();

                            phRegistrantControls.Controls.Add( ppMobile );

                            if ( setValue && fieldValue != null )
                            {
                                var value = fieldValue as PhoneNumber;
                                if ( value != null )
                                {
                                    ppMobile.CountryCode = value.CountryCode;
                                    ppMobile.Number = value.ToString();
                                }
                            }
                        }

                        break;
                    }
                case RegistrationPersonFieldType.HomePhone:
                    {
                        var dv = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME );
                        if ( dv != null )
                        {
                            var ppHome = new PhoneNumberBox();
                            ppHome.ID = "ppHome";
                            ppHome.Label = dv.Value;
                            ppHome.Required = field.IsRequired;
                            ppHome.ValidationGroup = BlockValidationGroup;
                            ppHome.CountryCode = PhoneNumber.DefaultCountryCode();

                            phRegistrantControls.Controls.Add( ppHome );

                            if ( setValue && fieldValue != null )
                            {
                                var value = fieldValue as PhoneNumber;
                                if ( value != null )
                                {
                                    ppHome.CountryCode = value.CountryCode;
                                    ppHome.Number = value.ToString();
                                }
                            }
                        }

                        break;
                    }

                case RegistrationPersonFieldType.WorkPhone:
                    {
                        var dv = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK );
                        if ( dv != null )
                        {
                            var ppWork = new PhoneNumberBox();
                            ppWork.ID = "ppWork";
                            ppWork.Label = dv.Value;
                            ppWork.Required = field.IsRequired;
                            ppWork.ValidationGroup = BlockValidationGroup;
                            ppWork.CountryCode = PhoneNumber.DefaultCountryCode();

                            phRegistrantControls.Controls.Add( ppWork );

                            if ( setValue && fieldValue != null )
                            {
                                var value = fieldValue as PhoneNumber;
                                if ( value != null )
                                {
                                    ppWork.CountryCode = value.CountryCode;
                                    ppWork.Number = value.ToString();
                                }
                            }
                        }

                        break;
                    }
            }
        }

        /// <summary>
        /// Creates the attribute field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="setValue">if set to <c>true</c> [set value].</param>
        /// <param name="fieldValue">The field value.</param>
        private void CreateAttributeField( RegistrationTemplateFormField field, bool setValue, object fieldValue )
        {
            if ( field.AttributeId.HasValue )
            {
                var attribute = AttributeCache.Read( field.AttributeId.Value );

                string value = string.Empty;
                if ( setValue && fieldValue != null )
                {
                    value = fieldValue.ToString();
                }

                attribute.AddControl( phRegistrantControls.Controls, value, BlockValidationGroup, setValue, true, field.IsRequired, null, string.Empty );
            }
        }

        /// <summary>
        /// Creates the fee field.
        /// </summary>
        /// <param name="fee">The fee.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        /// <param name="feeValues">The fee values.</param>
        private void CreateFeeField( RegistrationTemplateFee fee, bool setValues, List<FeeInfo> feeValues )
        {
            if ( fee.FeeType == RegistrationFeeType.Single )
            {
                string label = fee.Name;
                var cost = fee.CostValue.AsDecimalOrNull();
                if ( cost.HasValue && cost.Value != 0.0M )
                {
                    label = string.Format( "{0} ({1})", fee.Name, cost.Value.FormatAsCurrency());
                }

                if ( fee.AllowMultiple )
                {
                    // Single Option, Multi Quantity
                    var numUpDown = new NumberUpDown();
                    numUpDown.ID = "fee_" + fee.Id.ToString();
                    numUpDown.Label = label;
                    numUpDown.Minimum = 0;
                    phFees.Controls.Add( numUpDown );

                    if ( setValues && feeValues != null && feeValues.Any() )
                    {
                        numUpDown.Value = feeValues.First().Quantity;
                    }
                }
                else
                {
                    // Single Option, Single Quantity
                    var cb = new RockCheckBox();
                    cb.ID = "fee_" + fee.Id.ToString();
                    cb.Label = label;
                    cb.SelectedIconCssClass = "fa fa-check-square-o fa-lg";
                    cb.UnSelectedIconCssClass = "fa fa-square-o fa-lg";
                    phFees.Controls.Add( cb );

                    if ( setValues && feeValues != null && feeValues.Any() )
                    {
                        cb.Checked = feeValues.First().Quantity > 0;
                    }
                }
            }
            else
            {
                // Parse the options to get name and cost for each
                var options = new Dictionary<string, string>();
                string[] nameValues = fee.CostValue.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
                foreach ( string nameValue in nameValues )
                {
                    string[] nameAndValue = nameValue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );
                    if ( nameAndValue.Length == 1)
                    {
                        options.AddOrIgnore( nameAndValue[0], nameAndValue[0] );
                    }
                    if ( nameAndValue.Length == 2 )
                    {
                        options.AddOrIgnore( nameAndValue[0], string.Format( "{0} ({1})", nameAndValue[0], nameAndValue[1].AsDecimal().FormatAsCurrency() ) );
                    }
                }

                if ( fee.AllowMultiple )
                {
                    HtmlGenericControl feeAllowMultiple = new HtmlGenericControl( "div" );
                    phFees.Controls.Add( feeAllowMultiple );

                    feeAllowMultiple.AddCssClass( "feetype-allowmultiples" );

                    Label titleLabel = new Label();
                    feeAllowMultiple.Controls.Add( titleLabel );
                    titleLabel.CssClass = "control-label";
                    titleLabel.Text = fee.Name;

                    foreach( var optionKeyVal in options )
                    {
                        var numUpDown = new NumberUpDown();
                        numUpDown.ID = string.Format( "fee_{0}_{1}", fee.Id, optionKeyVal.Key );
                        numUpDown.Label = string.Format( "{0}", optionKeyVal.Value );
                        numUpDown.Minimum = 0;
                        numUpDown.CssClass = "fee-allowmultiple";
                        feeAllowMultiple.Controls.Add( numUpDown );

                        if ( setValues && feeValues != null && feeValues.Any() )
                        {
                            numUpDown.Value = feeValues
                                .Where( f => f.Option == optionKeyVal.Key )
                                .Select( f => f.Quantity )
                                .FirstOrDefault();
                        }
                    }
                }
                else
                {
                    // Multi Option, Single Quantity
                    var ddl = new RockDropDownList();
                    ddl.ID = "fee_" + fee.Id.ToString();
                    ddl.AddCssClass( "input-width-md" );
                    ddl.Label = fee.Name;
                    ddl.DataValueField = "Key";
                    ddl.DataTextField = "Value";
                    ddl.DataSource = options;
                    ddl.DataBind();
                    ddl.Items.Insert( 0, "");
                    phFees.Controls.Add( ddl );

                    if ( setValues && feeValues != null && feeValues.Any() )
                    {
                        ddl.SetValue( feeValues
                            .Where( f => f.Quantity > 0 )
                            .Select( f => f.Option )
                            .FirstOrDefault() );
                    }
                }
            }
        }

        /// <summary>
        /// Parses the registrant controls.
        /// </summary>
        private void ParseRegistrantControls()
        {
            if ( RegistrationState != null && RegistrationState.Registrants.Count > CurrentRegistrantIndex )
            {
                var registrant = RegistrationState.Registrants[CurrentRegistrantIndex];

                if ( rblFamilyOptions.Visible )
                {
                    registrant.FamilyGuid = rblFamilyOptions.SelectedValue.AsGuid();
                }

                if ( registrant.FamilyGuid.Equals( Guid.Empty ) )
                {
                    registrant.FamilyGuid = Guid.NewGuid();
                }

                var form = RegistrationTemplate.Forms.OrderBy( f => f.Order ).ToList()[CurrentFormIndex];
                foreach ( var field in form.Fields.Where( f => !f.IsInternal ).OrderBy( f => f.Order ) )
                {
                    object value = null;

                    if ( field.FieldSource == RegistrationFieldSource.PersonField )
                    {
                        value = ParsePersonField( field );
                    }
                    else
                    {
                        value = ParseAttributeField( field );
                    }

                    if ( value != null )
                    {
                        registrant.FieldValues.AddOrReplace( field.Id, new FieldValueObject( field, value ) );
                    }
                    else
                    {
                        registrant.FieldValues.Remove( field.Id );
                    }
                }

                if ( FormCount - 1 == CurrentFormIndex )
                {
                    foreach ( var fee in RegistrationTemplate.Fees )
                    {
                        List<FeeInfo> feeValues = ParseFee( fee );
                        if ( fee != null )
                        {
                            registrant.FeeValues.AddOrReplace( fee.Id, feeValues );
                        }
                        else
                        {
                            registrant.FeeValues.Remove( fee.Id );
                        }
                    }
                }

            }
        }

        /// <summary>
        /// Parses the person field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        private object ParsePersonField( RegistrationTemplateFormField field )
        {
            switch ( field.PersonFieldType )
            {
                case RegistrationPersonFieldType.FirstName:
                    {
                        var tbFirstName = phRegistrantControls.FindControl( "tbFirstName" ) as RockTextBox;
                        string value = tbFirstName != null ? tbFirstName.Text : null;
                        return string.IsNullOrWhiteSpace( value ) ? null : value;
                    }

                case RegistrationPersonFieldType.LastName:
                    {
                        var tbLastName = phRegistrantControls.FindControl( "tbLastName" ) as RockTextBox;
                        string value = tbLastName != null ? tbLastName.Text : null;
                        return string.IsNullOrWhiteSpace( value ) ? null : value;
                    }

                case RegistrationPersonFieldType.Campus:
                    {
                        var cpHomeCampus = phRegistrantControls.FindControl( "cpHomeCampus" ) as CampusPicker;
                        return cpHomeCampus != null ? cpHomeCampus.SelectedCampusId : null;
                    }

                case RegistrationPersonFieldType.Address:
                    {
                        var location = new Location();
                        var acAddress = phRegistrantControls.FindControl( "acAddress" ) as AddressControl;
                        if ( acAddress != null )
                        {
                            acAddress.GetValues( location );
                            return location;
                        }
                        break;
                    }
                   
                case RegistrationPersonFieldType.Email:
                    {
                        var tbEmail = phRegistrantControls.FindControl( "tbEmail" ) as EmailBox;
                        string value = tbEmail != null ? tbEmail.Text : null;
                        return string.IsNullOrWhiteSpace( value ) ? null : value;
                    }

                case RegistrationPersonFieldType.Birthdate:
                    {
                        var bpBirthday = phRegistrantControls.FindControl( "bpBirthday" ) as BirthdayPicker;
                        return bpBirthday != null ? bpBirthday.SelectedDate : null;
                    }

                case RegistrationPersonFieldType.Gender:
                    {
                        var ddlGender = phRegistrantControls.FindControl( "ddlGender" ) as RockDropDownList;
                        return ddlGender != null ? ddlGender.SelectedValueAsInt() : null;
                    }

                case RegistrationPersonFieldType.MaritalStatus:
                    {
                        var ddlMaritalStatus = phRegistrantControls.FindControl( "ddlMaritalStatus" ) as RockDropDownList;
                        return ddlMaritalStatus != null ? ddlMaritalStatus.SelectedValueAsInt() : null;
                    }

                case RegistrationPersonFieldType.MobilePhone:
                    {
                        var phoneNumber = new PhoneNumber();
                        var ppMobile = phRegistrantControls.FindControl( "ppMobile" ) as PhoneNumberBox;
                        if ( ppMobile != null )
                        {
                            phoneNumber.CountryCode = PhoneNumber.CleanNumber( ppMobile.CountryCode );
                            phoneNumber.Number = PhoneNumber.CleanNumber( ppMobile.Number );
                            return phoneNumber;
                        }
                        break;
                    }

                case RegistrationPersonFieldType.HomePhone:
                    {
                        var phoneNumber = new PhoneNumber();
                        var ppHome = phRegistrantControls.FindControl( "ppHome" ) as PhoneNumberBox;
                        if ( ppHome != null )
                        {
                            phoneNumber.CountryCode = PhoneNumber.CleanNumber( ppHome.CountryCode );
                            phoneNumber.Number = PhoneNumber.CleanNumber( ppHome.Number );
                            return phoneNumber;
                        }
                        break;
                    }

                case RegistrationPersonFieldType.WorkPhone:
                    {
                        var phoneNumber = new PhoneNumber();
                        var ppWork = phRegistrantControls.FindControl( "ppWork" ) as PhoneNumberBox;
                        if ( ppWork != null )
                        {
                            phoneNumber.CountryCode = PhoneNumber.CleanNumber( ppWork.CountryCode );
                            phoneNumber.Number = PhoneNumber.CleanNumber( ppWork.Number );
                            return phoneNumber;
                        }
                        break;
                    }
            }

            return null;

        }

        /// <summary>
        /// Parses the attribute field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        private object ParseAttributeField( RegistrationTemplateFormField field )
        {
            if ( field.AttributeId.HasValue )
            {
                var attribute = AttributeCache.Read( field.AttributeId.Value );
                string fieldId = "attribute_field_" + attribute.Id.ToString();

                Control control = phRegistrantControls.FindControl( fieldId );
                if ( control != null )
                {
                    return attribute.FieldType.Field.GetEditValue( control, attribute.QualifierValues );
                }
            }

            return null;
        }

        /// <summary>
        /// Parses the fee.
        /// </summary>
        /// <param name="fee">The fee.</param>
        /// <returns></returns>
        private List<FeeInfo> ParseFee( RegistrationTemplateFee fee )
        {
            string fieldId = string.Format( "fee_{0}", fee.Id );

            if ( fee.FeeType == RegistrationFeeType.Single )
            {
                if ( fee.AllowMultiple )
                {
                    // Single Option, Multi Quantity
                    var numUpDown = phFees.FindControl( fieldId ) as NumberUpDown;
                    if ( numUpDown != null && numUpDown.Value > 0 )
                    {
                        return new List<FeeInfo> { new FeeInfo( string.Empty, numUpDown.Value, fee.CostValue.AsDecimal() ) };
                    }
                }
                else
                {
                    // Single Option, Single Quantity
                    var cb = phFees.FindControl( fieldId ) as RockCheckBox;
                    if ( cb != null && cb.Checked )
                    {
                        return new List<FeeInfo> { new FeeInfo( string.Empty, 1, fee.CostValue.AsDecimal() ) };
                    }
                }
            }
            else
            {
                // Parse the options to get name and cost for each
                var options = new Dictionary<string, string>();
                var optionCosts = new Dictionary<string, decimal>();

                string[] nameValues = fee.CostValue.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
                foreach ( string nameValue in nameValues )
                {
                    string[] nameAndValue = nameValue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );
                    if ( nameAndValue.Length == 1 )
                    {
                        options.AddOrIgnore( nameAndValue[0], nameAndValue[0] );
                        optionCosts.AddOrIgnore( nameAndValue[0], 0.0m );
                    }
                    if ( nameAndValue.Length == 2 )
                    {
                        options.AddOrIgnore( nameAndValue[0], string.Format( "{0} ({1})", nameAndValue[0], nameAndValue[1].AsDecimal().FormatAsCurrency() ) );
                        optionCosts.AddOrIgnore( nameAndValue[0], nameAndValue[1].AsDecimal() );
                    }
                }

                if ( fee.AllowMultiple )
                {
                    // Multi Option, Multi Quantity
                    var result = new List<FeeInfo>();

                    foreach ( var optionKeyVal in options )
                    {
                        string optionFieldId = string.Format( "{0}_{1}", fieldId, optionKeyVal.Key );
                        var numUpDown = phFees.FindControl( optionFieldId ) as NumberUpDown;
                        if ( numUpDown != null && numUpDown.Value > 0 )
                        {
                            result.Add( new FeeInfo( optionKeyVal.Key, numUpDown.Value, optionCosts[optionKeyVal.Key] ) );
                        }
                    }

                    if ( result.Any() )
                    {
                        return result;
                    }
                }
                else
                {
                    // Multi Option, Single Quantity
                    var ddl = phFees.FindControl( fieldId ) as RockDropDownList;
                    if ( ddl != null && ddl.SelectedValue != "" )
                    {
                        return new List<FeeInfo> { new FeeInfo( ddl.SelectedValue, 1, optionCosts[ddl.SelectedValue] ) };
                    }
                }
            }

            return null;
        }

        #endregion

        #region Summary/Payment Controls

        private void CreateSummaryControls( bool setValues )
        {
            lDiscountCodeLabel.Text = DiscountCodeTerm;

            if ( RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Ask )
            {
                var familyOptions = RegistrationState.GetFamilyOptions( RegistrationTemplate, RegistrationState.RegistrantCount );
                if ( familyOptions.Any() )
                {
                    familyOptions.Add( familyOptions.ContainsKey( RegistrationState.FamilyGuid ) ?
                        Guid.NewGuid() :
                        RegistrationState.FamilyGuid.Equals( Guid.Empty ) ? Guid.NewGuid() : RegistrationState.FamilyGuid,
                        "None" );
                    rblRegistrarFamilyOptions.DataSource = familyOptions;
                    rblRegistrarFamilyOptions.DataBind();
                    pnlRegistrarFamilyOptions.Visible = true;
                }
                else
                {
                    pnlRegistrarFamilyOptions.Visible = false;
                }
            }
            else
            {
                pnlRegistrarFamilyOptions.Visible = false;
            }

            if ( setValues && RegistrationState != null )
            {
                // Check to see if this is an existing registration or information has already been entered
                if ( RegistrationState.RegistrationId.HasValue ||
                    !string.IsNullOrWhiteSpace( RegistrationState.FirstName) ||
                    !string.IsNullOrWhiteSpace( RegistrationState.LastName ) ||
                    !string.IsNullOrWhiteSpace( RegistrationState.ConfirmationEmail ) )
                {
                    // If so, use it
                    tbYourFirstName.Text = RegistrationState.FirstName;
                    tbYourLastName.Text = RegistrationState.LastName;
                    tbConfirmationEmail.Text = RegistrationState.ConfirmationEmail;
                }
                else
                {
                    if ( CurrentPerson != null )
                    {
                        tbYourFirstName.Text = CurrentPerson.NickName;
                        tbYourLastName.Text = CurrentPerson.LastName;
                        tbConfirmationEmail.Text = CurrentPerson.Email;
                    }
                    else
                    {
                        tbYourFirstName.Text = string.Empty;
                        tbYourLastName.Text = string.Empty;
                        tbConfirmationEmail.Text = string.Empty;
                    }
                }

                rblRegistrarFamilyOptions.SetValue( RegistrationState.FamilyGuid.ToString() );

                // Build Discount info
                nbDiscountCode.Visible = false;
                if ( RegistrationTemplate != null && RegistrationTemplate.Discounts.Any() )
                {
                    // Only allow discount code to be entered for a new registration
                    divDiscountCode.Visible = !RegistrationState.RegistrationId.HasValue;

                    string discountCode = RegistrationState.DiscountCode;
                    tbDiscountCode.Text = discountCode;
                    if ( !string.IsNullOrWhiteSpace( discountCode ))
                    {
                        var discount = RegistrationTemplate.Discounts
                            .Where( d => d.Code.Equals( discountCode, StringComparison.OrdinalIgnoreCase ) )
                            .FirstOrDefault();
                        if ( discount == null )
                        {
                            nbDiscountCode.Text = string.Format( "'{1}' is not a valid {1}.", discountCode, DiscountCodeTerm );
                            nbDiscountCode.Visible = true;
                        }

                    }
                }
                else
                {
                    divDiscountCode.Visible = false;
                }

                // Get the cost/fee summary
                var costs = new List<RegistrationCostSummaryInfo>();
                foreach( var registrant in RegistrationState.Registrants )
                {
                    if ( registrant.Cost > 0 )
                    {
                        var costSummary = new RegistrationCostSummaryInfo();
                        costSummary.Type = RegistrationCostSummaryType.Cost;
                        costSummary.Description = string.Format( "{0} {1}",
                            registrant.GetFirstName( RegistrationTemplate ),
                            registrant.GetLastName( RegistrationTemplate ) );
                        costSummary.Cost = registrant.Cost;
                        if ( RegistrationState.DiscountPercentage > 0.0m )
                        {
                            costSummary.DiscountedCost = costSummary.Cost - ( costSummary.Cost * RegistrationState.DiscountPercentage );
                        }
                        else
                        {
                            costSummary.DiscountedCost = costSummary.Cost;
                        }

                        // If registration allows a minimum payment calculate that amount, otherwise use the discounted amount as minimum
                        costSummary.MinPayment = RegistrationTemplate.MinimumInitialPayment.HasValue ? 
                            RegistrationTemplate.MinimumInitialPayment.Value : costSummary.DiscountedCost;

                        costs.Add( costSummary );
                    }

                    foreach( var fee in registrant.FeeValues )
                    {
                        var templateFee = RegistrationTemplate.Fees.Where( f => f.Id == fee.Key ).FirstOrDefault();
                        if ( fee.Value != null )
                        {
                            foreach ( var feeInfo in fee.Value )
                            {
                                decimal cost = feeInfo.PreviousCost > 0.0m ? feeInfo.PreviousCost : feeInfo.Cost;
                                string desc = string.Format( "{0}{1} ({2:N0} @ {3})",
                                    templateFee != null ? templateFee.Name : "(Previous Cost)",
                                    string.IsNullOrWhiteSpace( feeInfo.Option ) ? "" : "-" + feeInfo.Option,
                                    feeInfo.Quantity,
                                    cost.FormatAsCurrency() );

                                var costSummary = new RegistrationCostSummaryInfo();
                                costSummary.Type = RegistrationCostSummaryType.Fee;
                                costSummary.Description = desc;
                                costSummary.Cost = feeInfo.Quantity * cost;

                                if ( RegistrationState.DiscountPercentage > 0.0m && templateFee != null && templateFee.DiscountApplies )
                                {
                                    costSummary.DiscountedCost = costSummary.Cost - ( costSummary.Cost * RegistrationState.DiscountPercentage );
                                }
                                else
                                {
                                    costSummary.DiscountedCost = costSummary.Cost;
                                }

                                // If template allows a minimum payment, then fees are not included, otherwise it is included
                                costSummary.MinPayment = RegistrationTemplate.MinimumInitialPayment.HasValue ? 0 : costSummary.DiscountedCost;

                                costs.Add( costSummary );
                            }
                        }
                    }
                }

                minimumPayment = 0.0M;

                // If there were any costs
                if ( costs.Any() )
                {
                    pnlRegistrantsReview.Visible = false;
                    pnlMoney.Visible = true;

                    // Get the total min payment for all costs and fees
                    minimumPayment = costs.Sum( c => c.MinPayment );

                    // Add row for amount discount
                    if ( RegistrationState.DiscountAmount > 0.0m )
                    {
                        decimal totalDiscount = 0.0m - ( RegistrationState.Registrants.Count * RegistrationState.DiscountAmount );
                        costs.Add( new RegistrationCostSummaryInfo
                        {
                            Type = RegistrationCostSummaryType.Discount,
                            Description = "Discount",
                            Cost = totalDiscount,
                            DiscountedCost = totalDiscount
                        } );
                    }

                    // Get the totals
                    RegistrationState.TotalCost = costs.Sum( c => c.Cost );
                    RegistrationState.DiscountedCost = costs.Sum( c => c.DiscountedCost );

                    // If minimum payment is greater than total discounted cost ( which is possible with discounts ), adjust the minimum payment
                    minimumPayment = minimumPayment.Value > RegistrationState.DiscountedCost ? RegistrationState.DiscountedCost : minimumPayment;

                    // Add row for totals
                    costs.Add( new RegistrationCostSummaryInfo
                    {
                        Type = RegistrationCostSummaryType.Total,
                        Description = "Total",
                        Cost = costs.Sum( c => c.Cost ),
                        DiscountedCost = RegistrationState.DiscountedCost,
                    } );

                    rptFeeSummary.DataSource = costs;
                    rptFeeSummary.DataBind();

                    // Set the total cost
                    hfTotalCost.Value = RegistrationState.DiscountedCost.ToString();
                    lTotalCost.Text = RegistrationState.DiscountedCost.FormatAsCurrency();

                    // Check for previous payments
                    lPreviouslyPaid.Visible = RegistrationState.PreviousPaymentTotal != 0.0m;
                    hfPreviouslyPaid.Value = RegistrationState.PreviousPaymentTotal.ToString();
                    lPreviouslyPaid.Text = RegistrationState.PreviousPaymentTotal.FormatAsCurrency();
                    minimumPayment = minimumPayment.Value - RegistrationState.PreviousPaymentTotal;

                    // if min payment is less than 0, set it to 0
                    minimumPayment = minimumPayment.Value < 0 ? 0 : minimumPayment.Value;

                    // Calculate balance due, and if a partial payment is still allowed
                    decimal balanceDue = RegistrationState.DiscountedCost - RegistrationState.PreviousPaymentTotal;
                    bool allowPartialPayment = balanceDue > 0 && minimumPayment.Value < balanceDue;

                    // If partial payment is allowed, show the minimum payment due
                    lMinimumDue.Visible = allowPartialPayment;
                    hfMinimumDue.Value = minimumPayment.Value.ToString();
                    lMinimumDue.Text = minimumPayment.Value.FormatAsCurrency();

                    // Make sure payment amount is within minumum due and balance due. If not, set to balance due
                    if ( !RegistrationState.PaymentAmount.HasValue ||
                        RegistrationState.PaymentAmount.Value < minimumPayment.Value ||
                        RegistrationState.PaymentAmount.Value > balanceDue )
                    {
                        RegistrationState.PaymentAmount = balanceDue;
                    }

                    nbAmountPaid.Visible = allowPartialPayment;
                    nbAmountPaid.Text = ( RegistrationState.PaymentAmount ?? 0.0m ).ToString( "N2" );

                    // If a previous payment was made, or partial payment is allowed, show the amount remaining after selected payment amount
                    lRemainingDue.Visible = allowPartialPayment || RegistrationState.PreviousPaymentTotal != 0.0m;
                    lRemainingDue.Text = ( RegistrationState.DiscountedCost - ( RegistrationState.PreviousPaymentTotal + ( RegistrationState.PaymentAmount ?? 0.0m ) ) ).FormatAsCurrency();

                    divPaymentInfo.Visible = balanceDue > 0;

                    // Set payment options based on gateway settings
                    if ( balanceDue > 0 && RegistrationTemplate.FinancialGateway != null )
                    {
                        divPaymentInfo.Visible = true;

                        if ( RegistrationTemplate.FinancialGateway.Attributes == null )
                        {
                            RegistrationTemplate.LoadAttributes();
                        }

                        var component = RegistrationTemplate.FinancialGateway.GetGatewayComponent();
                        if ( component != null )
                        {
                            txtCardFirstName.Visible = component.SplitNameOnCard;
                            txtCardLastName.Visible = component.SplitNameOnCard;
                            txtCardName.Visible = !component.SplitNameOnCard;
                            mypExpiration.MinimumYear = RockDateTime.Now.Year;
                        }

                        BindSavedAccounts( component );

                        if ( rblSavedCC.Items.Count > 0 )
                        {
                            rblSavedCC.Items[0].Selected = true;
                            rblSavedCC.Visible = true;
                            divNewCard.Style[HtmlTextWriterStyle.Display] = "none";
                        }
                        else
                        {
                            rblSavedCC.Visible = false;
                            divNewCard.Style[HtmlTextWriterStyle.Display] = "block";
                        }
                    }
                    else
                    {
                        divPaymentInfo.Visible = false;
                    }
                }
                else
                {
                    pnlRegistrantsReview.Visible = true;

                    lRegistrantsReview.Text = string.Format( "<p>The following {0} will be registered for {1}:",
                        RegistrationTemplate.RegistrantTerm.PluralizeIf( RegistrationState.Registrants.Count > 0 ).ToLower(), RegistrationTemplate.Name );

                    rptrRegistrantReview.DataSource = RegistrationState.Registrants
                        .Select( r => new
                        {
                            RegistrantName = r.GetFirstName( RegistrationTemplate ) + " " + r.GetLastName( RegistrationTemplate )
                        } );
                    rptrRegistrantReview.DataBind(); 

                    RegistrationState.TotalCost = 0.0m;
                    RegistrationState.DiscountedCost = 0.0m;
                    pnlMoney.Visible = false;
                }
            }
        }

        private void BindSavedAccounts( GatewayComponent component )
        {
            rblSavedCC.Items.Clear();

            if ( CurrentPerson != null )
            {
                // Get the saved accounts for the currently logged in user
                var savedAccounts = new FinancialPersonSavedAccountService( new RockContext() )
                    .GetByPersonId( CurrentPerson.Id );

                var ccCurrencyType = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD ) );
                if ( component != null && component.SupportsSavedAccount( ccCurrencyType ) )
                {
                    rblSavedCC.DataSource = savedAccounts
                        .Where( a =>
                            a.FinancialGatewayId == RegistrationTemplate.FinancialGateway.Id &&
                            a.FinancialPaymentDetail != null &&
                            a.FinancialPaymentDetail.CurrencyTypeValueId == ccCurrencyType.Id )
                        .OrderBy( a => a.Name )
                        .Select( a => new
                        {
                            Id = a.Id,
                            Name = "Use " + a.Name + " (" + a.FinancialPaymentDetail.AccountNumberMasked + ")"
                        } ).ToList();
                    rblSavedCC.DataBind();
                    if ( rblSavedCC.Items.Count > 0 )
                    {
                        rblSavedCC.Items.Add( new ListItem( "Use a different card", "0" ) );
                    }
                }
            }
        }

        private void ParseSummaryControls()
        {
            if ( RegistrationState != null )
            {
                RegistrationState.FirstName = tbYourFirstName.Text;
                RegistrationState.LastName = tbYourLastName.Text;
                RegistrationState.ConfirmationEmail = tbConfirmationEmail.Text;

                if ( rblRegistrarFamilyOptions.Visible )
                {
                    RegistrationState.FamilyGuid = rblRegistrarFamilyOptions.SelectedValue.AsGuid();
                }

                if ( RegistrationState.FamilyGuid.Equals( Guid.Empty ) )
                {
                    RegistrationState.FamilyGuid = Guid.NewGuid();
                }

                if ( RegistrationState.DiscountCode != tbDiscountCode.Text.Trim() )
                {
                    RegistrationState.DiscountCode = tbDiscountCode.Text.Trim();
                    if ( !string.IsNullOrWhiteSpace( RegistrationState.DiscountCode ) )
                    {
                        var discount = RegistrationTemplate.Discounts
                            .Where( d => d.Code.Equals( RegistrationState.DiscountCode, StringComparison.OrdinalIgnoreCase ) )
                            .FirstOrDefault();
                        RegistrationState.DiscountPercentage = discount != null ? discount.DiscountPercentage : 0.0m;
                        RegistrationState.DiscountAmount = discount != null ? discount.DiscountAmount : 0.0m;
                    }
                    else
                    {
                        RegistrationState.DiscountPercentage = 0.0m;
                        RegistrationState.DiscountAmount = 0.0m;
                    }
                }

                RegistrationState.PaymentAmount = nbAmountPaid.Text.AsDecimal();
            }
        }

        #endregion

        #endregion

        #endregion

    }
}