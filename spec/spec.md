---
layout: default
title: eDAIS Introduction
---

# Introduction

eDAIS is a SOAP-based system for transferring information about development applications between heterogeneous systems. This site covers the use of the specification in NSW and the modifications ('specialisations') employed.

eDAIS in NSW is currently used for sending information about Complying Development between the various systems involved in assessing an application. Complying Development can be assessed by either the local Council, or by a Private Certifer that operates in that area. Private Certifiers do not need Council approval to Approve or Refuse an application, but they must notify Council two days prior to the commencement of building works.

There are three major categories of system in NSW:

* The Electronic Housing Code system (EHC), provided by [DP&I](http://www.planning.nsw.gov.au) and [GLS](http://www.licence.nsw.gov.au)
* Council systems, provided by commercial vendors
* Private Certifier systems, also provided by commercial vendors


This document uses the terms 'MUST', 'SHOULD' and 'MAY', and their negatives, in the manner described in [RFC 2119](http://tools.ietf.org/html/rfc2119).

### Glossary

Term                     | Definition
-------------------------|------------
Applicant                | The person submitting an application for work, such as a new swimming pool, or adding an extension to a house. Sometimes the owner, but often an architect or a home building company.
Owner                    | The person that owns the land parcel development is happening on.
Land Parcel              | A single piece of land, identified by a Land Title Reference.
Complying Development    | An alternative to Development Applications for fairly simple, non-controversial development. 10 day approvals. Can be approved by Council or by Private Certifiers.
Certifier                | The person or organisation responsible for approving or refusing a Complying Development Application.
Council Certifier        | The local Council can approve or refuse a Complying Development Application.
Private Certifier        | Private Certifiers can also approve or refuse Complying Development Applications, as long as they are accredited by the [BPB](http://www.bpb.nsw.gov.au/).
Certifier System         | A software system belonging to a Council or Private Certifier and participating in the exchange of eDAIS messages.
Council System           | The software system used by a local Council to manage Complying Development Applications.
Private Certifier System | The software system used by a Private Certifier to manage Complying Development Applications.
EHC                      | Used by Appliants to submit Complying Development Applications to Councils or Private Certifiers. See the [EHC website](http://www.electronichousingcode.com.au). Where the Council System or Private Certifier System are eDAIS-enabled, eDAIS messages will be used to communicate application creation & status.
Determine Application    | To assess an application and Approve or Refuse it. Approved applications are issued with a Complying Development Certificate.


## Application Process

The process, from an applicant's point of view:

Step                     | In Plain English 
-------------------------|------------------
Investigate              | Can I put in a pool?
Prepare Application      | Upload plans, provide contact details.
Submit for Consideration | Here's my application; check it over please. Certifier can Accept (take on the work) or Reject (refuse the work). Payment normally occurs at this step, and is managed out of band.
Lodge Application        | Formally start processing this application. This is the point at which the CDC 10-day clock starts.
Determine Application    | Approve or Refuse the application.


**Note:** A common point of confusion:

* Accept/Reject: Certifier decides if they wish to take on the business.
* Approve/Refuse: Certifier determines whether the development is allowed to proceed or not.

The applicant can choose who they wish to assess an application:
1. Local Council
2. Private Certifier

When an applicant Submits for Consideration using the EHC, they select the Certifier from a list of Accredited Certifers. The local Council is always one of these. 


### Council as Certifier

Step                     | eDAIS
-------------------------|------
Investigate              | -
Prepare Application      | -
Submit for Consideration | - 
Lodge Application        | EHC sends application to Council system
Determine Application    | Council system sends determination to the EHC

Detailed message flow is described [later on](#messages-council).

### EHC with Private Certifier and Council

Step                     | eDAIS
-------------------------|------
Investigate              | - 
Prepare Application      | - 
Submit for Consideration | - 
Lodge Application        | EHC sends application to Private Certifier system
Determine Application    | Private Certifier system sends determination to the EHC and the Council system

Detailed message flow is described [later on](#messages-certifier).

## Transport Layer & Security

The transport layer is SOAP with `BasicHttpBinding`. Messages are sent and acknowledged synchronously.

SSL/TLS is enforced, and the EHC will only connect to systems on the default SSL port (443). Deployments may terminate SSL at the network perimeter and forward messages over HTTP on their internal network.

Messages are additionally secured using [UsernameToken][1]. The username and password are configured on a per-endpoint basis, that is per-Certifier System. These usernames and passwords must be configured at both ends of any communication link.

{% highlight xml %}
<soapenv:Header>
    <wsse:Security 
      xmlns:wsse="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd" xmlns:u="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd">
      <u:Timestamp u:Id="89f3558b-91cd-426a-a7cc-e75e8a68eed2">
        <u:Created>2012-11-29T02:48:12.911Z</u:Created>
        <u:Expires>2012-11-29T02:53:12.911Z</u:Expires>
      </u:Timestamp>
      <wsse:UsernameToken>
        <wsse:Username>sample-username</wsse:Username>
        <wsse:Password Type="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText">sample-password</wsse:Password>
      </wsse:UsernameToken>
    </wsse:Security>
  </soapenv:Header>
{% endhighlight %}

[1]: http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0.pdf

## Attachments

Attachments are in the `ProposeCreateApplication` and `DeclareSaveDetermination` messages. Below is a sample block from a `ProposeCreateApplication`. 

{% highlight xml %}
<Attachment xmlns="http://xml.gov.au/edais/core/da.data.2.3.0r2">
  <Date>2012-11-29T13:22:10.7104852+11:00</Date>
  <Description>Investigation Report</Description>
  <Identification>EHC_Complying_Report.pdf</Identification>
  <Size UnitCode="bytes">120170</Size>
  <Type>Investigation Report</Type>
  <URI>https://uat.licence.nsw.gov.au/gls_portal/Attachment.mvc/Download?fid=99999&amp;s=xxxxx</URI>
</Attachment>
{% endhighlight %}

* Systems should retrieve attachments as soon as possible and must aim to do so within an hour of receiving a message. 
* Attachments sourced from the EHC will will expire after 3 retrieval attempts. There may also be a time expiry on this link, but that is currently unclear.
* Systems should attempt to use the `<Type>` to automatically categorise attachments.
* Systems must be able to handle the fact that the values provided in `<Type>` may change without warning.
* Systems must not notify end users of a new application or of a determination until attachments have been successfully retrieved. 
* Systems must raise an error for the attention of the appropriate humans if attachments are unable to be downloaded, either as the client or server.


## Messages

All of the messages below can be obtained by cloning this repo. Might be easier than clicking on each one.

### Message Flow - Council as Certifier {#messages-council}

TODO: message flow diagram

Step                     | Message                                                                                                     | Purpose
-------------------------|-------------------------------------------------------------------------------------------------------------|----------------------------------------------
Lodge Application        | [ProposeCreateApplicationTransaction](/edais/messages/council/01_ProposeCreateApplicationTransaction.xml)   | Creates an application in the Council system. 
 -                       | [ReceiptAcknowledgementSignal](/edais/messages/council/02_ReceiptAcknowledgementSignal.xml)                 | Acknowledges the receipt and creation of the application.
 -                       | TODO: ambiguity about AcceptCreateApplication? Is it required?                                                          | -
 -                       | TODO: Receipt for AcceptCreateApplication? Is it required?                                                                  | -
Determine Application    | [DeclareSaveDeterminationNotification](/edais/messages/council/03_DeclareSaveDeterminationNotification.xml) | Let the EHC know that the application has been approved or refused.
 -                       | [ReceiptAcknowledgementSignal](/edais/messages/council/04_ReceiptAcknowledgementSignal.xml)                 | Acknowledges the determination of the application.

### Message Flow - Private Certifer as Certifier {#messages-certifier}

TODO: message flow diagram

Step                     | Message                                                                                                                         | Purpose
-------------------------|---------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------
Lodge Application        | [ProposeCreateApplicationTransaction](/edais/messages/private-certifier/01_ProposeCreate_GLS_to_Buildaform.xml)                 | Creates an application in the Private Certifier system.
 -                       | [ReceiptAcknowledgementSignal](/edais/messages/private-certifier/01A_ReceiptAcknowledgement.xml)                                | Acknowledges the receipt of the application.
 -                       | [AcceptCreateApplication](/edais/messages/private-certifier/02_AcceptCreateApplication_Buildaform_to_GLS.xml)                   | Application has been successfully created with all attachments.
 -                       | [ReceiptAcknowledgementSignal](/edais/messages/private-certifier/02A_ReceiptAcknowledgement.xml)                                | Acknowledges the receipt of the `AcceptCreateApplication` message.
 -                       | [ProposeCreateApplicationTransaction](/edais/messages/private-certifier/03_ProposeCreateApplication_Buildaform_to_Council.xml)  | -
 -                       | [ReceiptAcknowledgementSignal](/edais/messages/private-certifier/03A_ReceiptAcknowledgement.xml)                                | Acknowledges the receipt of the `ProposeCreateApplicationTransaction` message.  
 -                       | [AcceptCreateApplication](/edais/messages/private-certifier/04_AcceptCreateApplication_Council_to_Buildaform.xml)               | Application has been successfully created with all attachments.
 -                       | [ReceiptAcknowledgementSignal](/edais/messages/private-certifier/04A_ReceiptAcknowledgement.xml)                                | Acknowledges the receipt of the `AcceptCreateApplication` message.
Determine Application    | [DeclareSaveDeterminationNotification](/edais/messages/private-certifier/05_DeclareSaveDetermination_Buildaform_to_GLS.xml)     | Let the EHC know that the application has been approved or refused.
 -                       | [ReceiptAcknowledgementSignal](/edais/messages/private-certifier/05A_ReceiptAcknowledgement.xml)                                | Acknowledges the determination of the application.
 -                       | [DeclareSaveDeterminationNotification](/edais/messages/private-certifier/06_DeclareSaveDetermination_Buildaform_to_Council.xml) | Let the local Council know that the application has been approved. Must not be sent for refusals.
 -                       | [ReceiptAcknowledgementSignal](/edais/messages/private-certifier/06A_ReceiptAcknowledgement.xml)                                | Acknowledges the determination of the application.

 



















