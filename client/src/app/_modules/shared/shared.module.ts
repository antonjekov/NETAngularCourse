import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ToastrModule } from 'ngx-toastr';
import { NgxGalleryModule } from "@kolkov/ngx-gallery";
import { FileUploadModule } from 'ng2-file-upload';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { BsDatepickerModule } from 'ngx-bootstrap/datepicker'
import { TabsModule } from "ngx-bootstrap/tabs";
import {PaginationModule } from "ngx-bootstrap/pagination";
import { ButtonsModule } from 'ngx-bootstrap/buttons';
import { TimeagoModule } from 'ngx-timeago';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    BrowserModule,
    BrowserAnimationsModule,
    BsDropdownModule.forRoot(),
    BsDatepickerModule.forRoot(),
    PaginationModule.forRoot(),
    ButtonsModule.forRoot(),
    ToastrModule.forRoot({
      positionClass: 'toast-bottom-right'
    }),
    TabsModule.forRoot(),
    TimeagoModule.forRoot(),
    NgxGalleryModule,
    FileUploadModule
  ],
  exports: [
    BsDropdownModule,
    BsDatepickerModule,
    PaginationModule,
    ButtonsModule,
    CommonModule,
    BrowserModule,
    BrowserAnimationsModule,
    ToastrModule,
    TabsModule,
    NgxGalleryModule,
    FileUploadModule,
    TimeagoModule
  ]
})
export class SharedModule { }
