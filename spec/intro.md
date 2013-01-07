---
layout: default
title: eDAIS Introduction
---

# Introduction

eDAIS is a SOAP-based system for transferring information about development applications between heterogeneous systems. This site covers the use of the specification in NSW and the modifications ('specialisations') employed.

There are three categories of actor in NSW:

* The Electronic Housing Code system, provided by [DP&I](http://www.planning.nsw.gov.au) and [GLS](http://www.licence.nsw.gov.au)
* Council systems, provided by commercial vendors
* Private Certifier systems, also provided by commercial vendors

Complying Development can be certified by either the Council of the area the development is being undertaken in, or by a Private Certifer who operates in that area. Private Certifiers do not need Council approval to Approve or Refuse an application, but they must notify Council two days prior to the commencement of building works.

This document uses the terms 'MUST', 'SHOULD' and 'MAY', and their negatives, in the manner described in [RFC 2119](http://tools.ietf.org/html/rfc2119).

### Glossary

* Applicant: a person submitting an application for work, such as a new swimming pool, or adding an extension to a house. Often an architect or a home building company.
* System: a software system participating in the exchange of eDAIS messages.
* EHC: the system that accepts applications from applicants and initiates the eDAIS message flow.
* Council System: the system installed at Council to manage 


## Use Cases

The process, from an applicant's point of view:

Number | Step                     | In Plain English 
-------|--------------------------|------------------
1      | Investigate              | Can I put in a pool?
2      | Prepare Application      | Upload plans, provide contact details.
3      | Submit for Consideration | Here's my application; check it over please. Certifier can Accept (take on the work) or Reject (refuse the work)
4      | Lodge Application        | Formally start processing this application. This is the point at which the CDC 10-day clock starts.
5      | Determine Application    | Approve or Refuse the application.


**Note:** A common point of confusion:

* Accept/Reject: Certifier decides if they wish to take on the business.
* Approve/Refuse: Certifier determines whether the application is allowed or not.

### Common Configurations

There are two primary configurations:
1. Council providing CDC.
2. Private Certifier providing CDC.

When an applicant Submits for Consideration, they select the Certifier from a list of Accredited Certifers. Council is one of these. 


### Council as Certifier

Number | Step                     | eDAIS
-------|--------------------------|------
1      | Investigate              | n/a
2      | Prepare Application      | n/a
3      | Submit for Consideration | n/a 
4      | Lodge Application        | EHC sends application to Council system
5      | Determine Application    | Council system sends determination to the EHC

### EHC with Private Certifier and Council

Number | Step                     | eDAIS
-------|--------------------------|------
1      | Investigate              | n/a 
2      | Prepare Application      | n/a 
3      | Submit for Consideration | n/a 
4      | Lodge Application        | EHC sends application to Private Certifier system
5      | Determine Application    | Private Certifier system sends determination to the EHC and the Council system


## Transport Layer & Security

The transport layer is SOAP with `BasicHttpBinding`. Messages are sent and acknowledged synchronously.

SSL/TLS is enforced, and the EHC system can only connect to systems on the default SSL port (443). Deployments may terminate SSL at the network perimeter and forward messages over HTTP.

Messages are additionally secured using [UsernameToken][1]. The username and password are configured on a per-endpoint basis, that is per-Council and per-Private Certifier. These usernames and passwords must be identically configured in any communicating systems.

{% highlight xml %}
<soapenv:Header>
    <wsse:Security xmlns:wsse="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd" xmlns:u="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd">
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

Acknowledgements should be sent synchronously on the same connection.


[1]: http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0.pdf

## Attachments

Attachments are in the `ProposeCreateApplication` and `DeclareSaveDetermination` messages. Below is a sample block from a `ProposeCreateApplication`. 

EHC NOTE: Attachments sourced from the EHC will will expire after 3 retrieval attempts. There may also be a time expiry on this link, but that is currently unclear.

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
* Systems should attempt to use the `<Type>` to automatically categorise attachments.
* Systems must be able to handle the fact that the values in `<Type>` may change without warning.
* Systems must not notify end users of a new application or of a determination until attachments have been successfully retrieved. 
* Systems must raise an error for the attention of the appropriate humans if attachments are unable to be downloaded.


## Messages

'ProposeCreateApplication' is sent from the EHC to a Council or Private Certifier system. Synchronously, the Council or Private Certifier system should respond with a `ReceiptAcknowledgementSignal'.

Council or Private Certifier systems


## Private Certifier systems

* PC Systems







