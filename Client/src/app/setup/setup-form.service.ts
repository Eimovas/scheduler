import { Injectable } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';

@Injectable({
    providedIn: 'root'
})
export class SetupFormService {
    private _setupForm: FormGroup;

    get setupForm(): FormGroup {
        return this._setupForm;
    }

    constructor(fb: FormBuilder) { 
        this._setupForm = fb.group({
            operationTimes: fb.array([]),
            surgeries: fb.array([]),
            employeeSetup: fb.array([])
        });
    }

    public enable(): void {
        this._setupForm.enable();
    }
    
    public disable(): void {
        this._setupForm.disable();
    }
}
