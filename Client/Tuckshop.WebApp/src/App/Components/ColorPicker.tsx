import React from "react";
import { HexColorPicker } from "react-colorful";
import "../Styles/Components/ColorPicker.scss";

export default function ColorPicker (props: { color: string, onChange: (hex: string) => void }): React.ReactNode {

    const { color, onChange } = props;

    return (
        <>
            <HexColorPicker color={color} onChange={onChange} />
            <div className="color-controls">
                <input className="form-control color-hex-input" value={color} placeholder="Hex" onChange={e => onChange(e.target.value)} />
                <div className="color-preview" style={{ backgroundColor: color }} />
            </div>
        </>)
}